using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Response;
using static MAX.Bot.FrameworkSpecificMethods;

namespace MAX.Bot.Modules;

internal sealed class UpdatesModule : IUpdatesModule
{
    private readonly IMaxApiTransport _core;
    private readonly IMaxBotClient _client;

    internal UpdatesModule(IMaxApiTransport core, IMaxBotClient client)
    {
        _core = core;
        _client = client;
    }

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
        var queryParams = new Dictionary<string, string>();

        if (request.Limit.HasValue)
            queryParams["limit"] = request.Limit.Value.ToString();

        if (request.Timeout.HasValue)
            queryParams["timeout"] = request.Timeout.Value.ToString();

        if (request.Marker.HasValue)
            queryParams["marker"] = request.Marker.Value.ToString();

        if (request.Types != null && request.Types.Count != 0)
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
        long? currentMarker = marker;
        var consecutiveErrors = 0;
        var random = new Random();

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

                consecutiveErrors = 0; // Сбрасываем счётчик при успешном запросе

                foreach (var update in response.Updates)
                {
                    if (parallelProcessing)
                    {
                        // Неблокирующая параллельная обработка: медленный обработчик не подвешивает опрос очереди
                        _ = ProcessUpdateAsync(update, callback, cancellationToken);
                    }
                    else
                    {
                        await ProcessUpdateAsync(update, callback, cancellationToken).ConfigureAwait(false);
                    }
                }

                currentMarker = response.Marker;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                consecutiveErrors++;

                if (OnPollingError != null)
                {
                    try
                    {
                        await InvokeErrorAsync(OnPollingError, ex).ConfigureAwait(false);
                    }
                    catch
                    {
                        // Игнорируем исключения внутри пользовательского обработчика ошибок
                    }
                }

                // Экспоненциальная задержка с джиттером (максимум 5 секунд вместо 60 для быстрого восстановления)
                var delay = Math.Min(1000 * Math.Pow(1.5, consecutiveErrors - 1), 5000);
                var jitter = random.Next(0, 500);
                await Task.Delay((int)delay + jitter, cancellationToken).ConfigureAwait(false);
            }
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
            // Изоляция ошибок в пользовательских обработчиках событий (защита от Poison Pill DoS).
            // Ошибка в обработке одного сообщения не должна останавливать цикл и блокировать продвижение маркера.
            if (OnPollingError != null)
            {
                try
                {
                    await InvokeErrorAsync(OnPollingError, ex).ConfigureAwait(false);
                }
                catch
                {
                    // Игнорируем исключения внутри пользовательского обработчика ошибок
                }
            }
        }
    }

    public async Task DispatchUpdateAsync(Update update)
    {
        await InvokeAsync(OnUpdate, update, _client).ConfigureAwait(false);

        switch (update)
        {
            case MessageCreatedUpdate messageCreated:
                await InvokeAsync(OnMessageCreated, messageCreated, _client).ConfigureAwait(false);
                break;
            case MessageEditedUpdate messageEdited:
                await InvokeAsync(OnMessageEdited, messageEdited, _client).ConfigureAwait(false);
                break;
            case MessageRemovedUpdate messageRemoved:
                await InvokeAsync(OnMessageRemoved, messageRemoved, _client).ConfigureAwait(false);
                break;
            case MessageCallbackUpdate messageCallback:
                await InvokeAsync(OnMessageCallback, messageCallback, _client).ConfigureAwait(false);
                break;
            case BotAddedToChatUpdate botAdded:
                await InvokeAsync(OnBotAddedToChat, botAdded, _client).ConfigureAwait(false);
                break;
            case BotStartedUpdate botStarted:
                await InvokeAsync(OnBotStarted, botStarted, _client).ConfigureAwait(false);
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
            case UserAddedToChatUpdate userAdded:
                await InvokeAsync(OnUserAddedToChat, userAdded, _client).ConfigureAwait(false);
                break;
            case UserRemovedFromChatUpdate userRemoved:
                await InvokeAsync(OnUserRemovedFromChat, userRemoved, _client).ConfigureAwait(false);
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
            case CommentCreatedUpdate commentCreated:
                await InvokeAsync(OnCommentCreated, commentCreated, _client).ConfigureAwait(false);
                break;
            case CommentEditedUpdate commentEdited:
                await InvokeAsync(OnCommentEdited, commentEdited, _client).ConfigureAwait(false);
                break;
            case CommentRemovedUpdate commentRemoved:
                await InvokeAsync(OnCommentRemoved, commentRemoved, _client).ConfigureAwait(false);
                break;
            case BotAdminPermissionsChangedUpdate botAdminPermissionsChanged:
                await InvokeAsync(OnBotAdminPermissionsChanged, botAdminPermissionsChanged, _client).ConfigureAwait(false);
                break;
        }
    }

    private static async Task InvokeAsync<TUpdate>(Func<TUpdate, IMaxBotClient, Task>? handler, TUpdate update, IMaxBotClient client)
    {
        if (handler == null) return;
        foreach (Func<TUpdate, IMaxBotClient, Task> singleCast in handler.GetInvocationList().Cast<Func<TUpdate, IMaxBotClient, Task>>())
        {
            await singleCast(update, client);
        }
    }

    private static async Task InvokeErrorAsync(Func<Exception, Task>? handler, Exception ex)
    {
        if (handler == null) return;
        foreach (Func<Exception, Task> singleCast in handler.GetInvocationList().Cast<Func<Exception, Task>>())
        {
            await singleCast(ex);
        }
    }
}
