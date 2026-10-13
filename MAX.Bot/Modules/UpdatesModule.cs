using System.Globalization;
using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Response;
using static MAX.Bot.FrameworkSpecificMethods;

namespace MAX.Bot.Modules;

internal sealed class UpdatesModule : IUpdatesModule
{
    private const int MaxBackoffMs = 5000;
    private const int MaxJitterMs = 500;
    private const int MaxBackoffExponent = 30; // защита от переполнения счётчика ошибок
    private static readonly TimeSpan ShutdownGracePeriod = TimeSpan.FromSeconds(10);

    private readonly IMaxApiTransport _core;
    private readonly IMaxBotClient _client;

    internal UpdatesModule(IMaxApiTransport core, IMaxBotClient client)
    {
        _core = core;
        _client = client;
    }

    /// <summary>
    /// Максимум одновременно обрабатываемых обновлений в параллельном режиме.
    /// Ограничивает рост задач и нагрузку на пул потоков при всплеске входящих сообщений.
    /// </summary>
    public int MaxConcurrency { get; set; } = 32;

    public event Func<Update, IMaxBotClient, Task>? OnUpdate;
    public event Func<MessageCreatedUpdate, IMaxBotClient, Task>? OnMessageCreated;
    public event Func<MessageEditedUpdate, IMaxBotClient, Task>? OnMessageEdited;
    public event Func<MessageRemovedUpdate, IMaxBotClient, Task>? OnMessageRemoved;
    public event Func<MessageCallbackUpdate, IMaxBotClient, Task>? OnMessageCallback;
    public event Func<BotAddedToChatUpdate, IMaxBotClient, Task>? OnBotAddedToChat;
    public event Func<BotStartedUpdate, IMaxBotClient, Task>? OnBotStarted;
    public event Func<BotStoppedUpdate, IMaxBotClient, Task>? OnBotStopped;
    public event Func<BotRemovedFromChatUpdate, IMaxBotClient, Task>? OnBotRemovedFromChat;
    public event Func<ChatTitleChangedUpdate, IMaxBotClient, Task>? OnChatTitleChanged;
    public event Func<UserAddedToChatUpdate, IMaxBotClient, Task>? OnUserAddedToChat;
    public event Func<UserRemovedFromChatUpdate, IMaxBotClient, Task>? OnUserRemovedFromChat;
    public event Func<DialogClearedUpdate, IMaxBotClient, Task>? OnDialogCleared;
    public event Func<DialogMutedUpdate, IMaxBotClient, Task>? OnDialogMuted;
    public event Func<DialogUnmutedUpdate, IMaxBotClient, Task>? OnDialogUnmuted;
    public event Func<DialogRemovedUpdate, IMaxBotClient, Task>? OnDialogRemoved;
    public event Func<CommentCreatedUpdate, IMaxBotClient, Task>? OnCommentCreated;
    public event Func<CommentEditedUpdate, IMaxBotClient, Task>? OnCommentEdited;
    public event Func<CommentRemovedUpdate, IMaxBotClient, Task>? OnCommentRemoved;
    public event Func<BotAdminPermissionsChangedUpdate, IMaxBotClient, Task>? OnBotAdminPermissionsChanged;
    public event Func<Exception, Task>? OnPollingError;

    public async Task<GetUpdatesResponse> GetUpdatesAsync(
        GetUpdatesRequest request,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>(4);

        if (request.Limit.HasValue)
            queryParams["limit"] = request.Limit.Value.ToString(CultureInfo.InvariantCulture);

        if (request.Timeout.HasValue)
            queryParams["timeout"] = request.Timeout.Value.ToString(CultureInfo.InvariantCulture);

        if (request.Marker.HasValue)
            queryParams["marker"] = request.Marker.Value.ToString(CultureInfo.InvariantCulture);

        if (request.Types is { Count: > 0 })
            queryParams["types"] = string.Join(",", request.Types);

        var queryString = MaxBotCore.BuildQueryString(queryParams);

        return await _core.SendRequestAsync<GetUpdatesResponse>(
            HttpMethod.Get, $"/updates{queryString}", null, cancellationToken).ConfigureAwait(false);
    }

    public Task PollUpdatesWithCallback(
        Func<Update, IMaxBotClient, Task>? callback = null,
        int? limit = 100,
        int? timeout = 30,
        long? marker = null,
        List<string>? types = null,
        CancellationToken cancellationToken = default)
        => PollUpdatesWithCallback(callback, limit, timeout, marker, types, parallelProcessing: true, cancellationToken);

    public async Task PollUpdatesWithCallback(
        Func<Update, IMaxBotClient, Task>? callback,
        int? limit,
        int? timeout,
        long? marker,
        List<string>? types,
        bool parallelProcessing,
        CancellationToken cancellationToken = default)
    {
        var currentMarker = marker;
        var consecutiveErrors = 0;
        var random = new Random();

        var concurrency = Math.Max(1, MaxConcurrency);
        var throttle = parallelProcessing ? new SemaphoreSlim(concurrency, concurrency) : null;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var response = await GetUpdatesAsync(new GetUpdatesRequest
                    {
                        Limit = limit,
                        Timeout = timeout,
                        Marker = currentMarker,
                        Types = types
                    }, cancellationToken).ConfigureAwait(false);

                    consecutiveErrors = 0;

                    if (response.Updates is not null)
                    {
                        foreach (var update in response.Updates)
                        {
                            if (throttle is null)
                            {
                                await ProcessUpdateAsync(update, callback, cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                await throttle.WaitAsync(cancellationToken).ConfigureAwait(false);
                                _ = RunThrottledAsync(update, callback, throttle, cancellationToken);
                            }
                        }
                    }

                    // Не сбрасываем маркер, если сервер не прислал новый (иначе получим дубликаты)
                    if (response.Marker is long newMarker)
                        currentMarker = newMarker;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    consecutiveErrors = Math.Min(consecutiveErrors + 1, MaxBackoffExponent);

                    await ReportErrorAsync(ex).ConfigureAwait(false);

                    // Экспоненциальная задержка с джиттером
                    var delay = Math.Min(1000 * Math.Pow(1.5, consecutiveErrors - 1), MaxBackoffMs);
                    var wait = (int)delay + random.Next(0, MaxJitterMs);

                    try
                    {
                        await Task.Delay(wait, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // Отмена во время паузы — штатный выход, а не исключение наружу
                        break;
                    }
                }
            }
        }
        finally
        {
            // Graceful shutdown: ждём уже запущенные обработчики, но не бесконечно
            if (throttle is not null)
                await DrainAsync(throttle, concurrency).ConfigureAwait(false);
        }
    }

    private async Task RunThrottledAsync(
        Update update,
        Func<Update, IMaxBotClient, Task>? callback,
        SemaphoreSlim throttle,
        CancellationToken cancellationToken)
    {
        try
        {
            // Уходим с потока опроса, чтобы синхронная часть обработчика его не блокировала
            await Task.Yield();
            await ProcessUpdateAsync(update, callback, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            throttle.Release();
        }
    }

    private static async Task DrainAsync(SemaphoreSlim throttle, int concurrency)
    {
        var deadline = DateTime.UtcNow + ShutdownGracePeriod;

        for (var i = 0; i < concurrency; i++)
        {
            var left = deadline - DateTime.UtcNow;
            if (left <= TimeSpan.Zero || !await throttle.WaitAsync(left).ConfigureAwait(false))
                break;
        }
    }

    private async Task ProcessUpdateAsync(
        Update update,
        Func<Update, IMaxBotClient, Task>? callback,
        CancellationToken cancellationToken)
    {
        try
        {
            await DispatchUpdateAsync(update).ConfigureAwait(false);

            if (callback != null)
                await callback.Invoke(update, _client).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Штатная отмена
        }
        catch (Exception ex)
        {
            // Изоляция ошибок пользовательских обработчиков (защита от Poison Pill):
            // сбой одного сообщения не останавливает цикл и не блокирует продвижение маркера.
            await ReportErrorAsync(ex).ConfigureAwait(false);
        }
    }

    private async Task ReportErrorAsync(Exception ex)
    {
        var handler = OnPollingError;
        if (handler is null) return;

        try
        {
            await InvokeErrorAsync(handler, ex).ConfigureAwait(false);
        }
        catch
        {
            // Исключения внутри пользовательского обработчика ошибок игнорируем
        }
    }

    public async Task DispatchUpdateAsync(Update update)
    {
        if (update is null)
            throw new ArgumentNullException(nameof(update));

        await InvokeAsync(OnUpdate, update, _client).ConfigureAwait(false);

        // Порядок case — от самых частых событий к редким
        switch (update)
        {
            case MessageCreatedUpdate messageCreated:
                await InvokeAsync(OnMessageCreated, messageCreated, _client).ConfigureAwait(false);
                break;
            case MessageCallbackUpdate messageCallback:
                await InvokeAsync(OnMessageCallback, messageCallback, _client).ConfigureAwait(false);
                break;
            case MessageEditedUpdate messageEdited:
                await InvokeAsync(OnMessageEdited, messageEdited, _client).ConfigureAwait(false);
                break;
            case MessageRemovedUpdate messageRemoved:
                await InvokeAsync(OnMessageRemoved, messageRemoved, _client).ConfigureAwait(false);
                break;
            case BotStartedUpdate botStarted:
                await InvokeAsync(OnBotStarted, botStarted, _client).ConfigureAwait(false);
                break;
            case BotAddedToChatUpdate botAdded:
                await InvokeAsync(OnBotAddedToChat, botAdded, _client).ConfigureAwait(false);
                break;
            case UserAddedToChatUpdate userAdded:
                await InvokeAsync(OnUserAddedToChat, userAdded, _client).ConfigureAwait(false);
                break;
            case UserRemovedFromChatUpdate userRemoved:
                await InvokeAsync(OnUserRemovedFromChat, userRemoved, _client).ConfigureAwait(false);
                break;
            case BotStoppedUpdate botStopped:
                await InvokeAsync(OnBotStopped, botStopped, _client).ConfigureAwait(false);
                break;
            case BotRemovedFromChatUpdate botRemoved:
                await InvokeAsync(OnBotRemovedFromChat, botRemoved, _client).ConfigureAwait(false);
                break;
            case ChatTitleChangedUpdate titleChanged:
                await InvokeAsync(OnChatTitleChanged, titleChanged, _client).ConfigureAwait(false);
                break;
            case CommentCreatedUpdate commentCreated:
                await InvokeAsync(OnCommentCreated, commentCreated, _client).ConfigureAwait(false);
                break;
            case CommentEditedUpdate commentEdited:
                await InvokeAsync(OnCommentEdited, commentEdited, _client).ConfigureAwait(false);
                break;
            case CommentRemovedUpdate commentRemoved:
                await InvokeAsync(OnCommentRemoved, commentRemoved, _client).ConfigureAwait(false);
                break;
            case DialogClearedUpdate dialogCleared:
                await InvokeAsync(OnDialogCleared, dialogCleared, _client).ConfigureAwait(false);
                break;
            case DialogMutedUpdate dialogMuted:
                await InvokeAsync(OnDialogMuted, dialogMuted, _client).ConfigureAwait(false);
                break;
            case DialogUnmutedUpdate dialogUnmuted:
                await InvokeAsync(OnDialogUnmuted, dialogUnmuted, _client).ConfigureAwait(false);
                break;
            case DialogRemovedUpdate dialogRemoved:
                await InvokeAsync(OnDialogRemoved, dialogRemoved, _client).ConfigureAwait(false);
                break;
            case BotAdminPermissionsChangedUpdate botAdminPermissionsChanged:
                await InvokeAsync(OnBotAdminPermissionsChanged, botAdminPermissionsChanged, _client).ConfigureAwait(false);
                break;
        }
    }

    // Не async: для обычного случая (один подписчик) возвращаем Task обработчика напрямую,
    // без лишней машины состояний.
    private static Task InvokeAsync<TUpdate>(
        Func<TUpdate, IMaxBotClient, Task>? handler, TUpdate update, IMaxBotClient client)
    {
        if (handler is null) return Task.CompletedTask;

        var list = handler.GetInvocationList();
        if (list.Length == 1)
            return handler(update, client);

        return InvokeMulticastAsync(list, update, client);
    }

    private static async Task InvokeMulticastAsync<TUpdate>(
        Delegate[] list, TUpdate update, IMaxBotClient client)
    {
        List<Exception>? errors = null;

        // Ошибка одного подписчика не должна лишать остальных события
        foreach (var d in list)
        {
            try
            {
                await ((Func<TUpdate, IMaxBotClient, Task>)d)(update, client).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                (errors ??= new List<Exception>()).Add(ex);
            }
        }

        if (errors is not null)
            throw new AggregateException(errors);
    }

    private static async Task InvokeErrorAsync(Func<Exception, Task> handler, Exception ex)
    {
        foreach (var d in handler.GetInvocationList())
        {
            try
            {
                await ((Func<Exception, Task>)d)(ex).ConfigureAwait(false);
            }
            catch
            {
                // Один сломанный обработчик ошибок не мешает остальным
            }
        }
    }
}