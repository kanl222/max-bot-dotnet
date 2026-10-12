using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Attachment;
using MAX.Bot.Modules;
using static MAX.Bot.FrameworkSpecificMethods;

namespace MAX.Bot;

/// <summary>
/// Фасад-клиент для API MAX Bot.
/// Все методы сгруппированы по модулям: Messages, Chats, Members, Attachments, Updates, Subscriptions.
/// </summary>
public class MaxBotClient : IMaxBotClient, IDisposable
{
    /// <summary>Базовый URL API MAX Bot.</summary>
    public const string BaseUrl = "https://platform-api2.max.ru";

    private readonly IMaxApiTransport _transport;
    private readonly object _pollLock = new();
    private CancellationTokenSource? _pollCts;
    private Task? _pollTask;
    private bool _disposed;

    /// <inheritdoc/>
    public IMessagesModule Messages { get; private set; } = null!;

    /// <inheritdoc/>
    public IChatsModule Chats { get; private set; } = null!;

    /// <inheritdoc/>
    public IMembersModule Members { get; private set; } = null!;

    /// <inheritdoc/>
    public IAttachmentsModule Attachments { get; private set; } = null!;

    /// <inheritdoc/>
    public IUpdatesModule Updates { get; private set; } = null!;

    /// <inheritdoc/>
    public ISubscriptionsModule Subscriptions { get; private set; } = null!;

    /// <inheritdoc/>
    public ICommentsModule Comments { get; private set; } = null!;

    /// <inheritdoc/>
    public event Func<Update, IMaxBotClient, Task>? OnUpdate
    {
        add => Updates.OnUpdate += value;
        remove => Updates.OnUpdate -= value;
    }

    /// <inheritdoc/>
    public event Func<MessageCreatedUpdate, IMaxBotClient, Task>? OnMessageCreated
    {
        add => Updates.OnMessageCreated += value;
        remove => Updates.OnMessageCreated -= value;
    }

    /// <inheritdoc/>
    public event Func<MessageEditedUpdate, IMaxBotClient, Task>? OnMessageEdited
    {
        add => Updates.OnMessageEdited += value;
        remove => Updates.OnMessageEdited -= value;
    }

    /// <inheritdoc/>
    public event Func<MessageRemovedUpdate, IMaxBotClient, Task>? OnMessageRemoved
    {
        add => Updates.OnMessageRemoved += value;
        remove => Updates.OnMessageRemoved -= value;
    }

    /// <inheritdoc/>
    public event Func<MessageCallbackUpdate, IMaxBotClient, Task>? OnMessageCallback
    {
        add => Updates.OnMessageCallback += value;
        remove => Updates.OnMessageCallback -= value;
    }

    /// <inheritdoc/>
    public event Func<BotAddedToChatUpdate, IMaxBotClient, Task>? OnBotAddedToChat
    {
        add => Updates.OnBotAddedToChat += value;
        remove => Updates.OnBotAddedToChat -= value;
    }

    /// <inheritdoc/>
    public event Func<BotStartedUpdate, IMaxBotClient, Task>? OnBotStarted
    {
        add => Updates.OnBotStarted += value;
        remove => Updates.OnBotStarted -= value;
    }

    /// <inheritdoc/>
    public event Func<BotStoppedUpdate, IMaxBotClient, Task>? OnBotStopped
    {
        add => Updates.OnBotStopped += value;
        remove => Updates.OnBotStopped -= value;
    }

    /// <inheritdoc/>
    public event Func<BotRemovedFromChatUpdate, IMaxBotClient, Task>? OnBotRemovedFromChat
    {
        add => Updates.OnBotRemovedFromChat += value;
        remove => Updates.OnBotRemovedFromChat -= value;
    }

    /// <inheritdoc/>
    public event Func<ChatTitleChangedUpdate, IMaxBotClient, Task>? OnChatTitleChanged
    {
        add => Updates.OnChatTitleChanged += value;
        remove => Updates.OnChatTitleChanged -= value;
    }

    /// <inheritdoc/>
    public event Func<UserAddedToChatUpdate, IMaxBotClient, Task>? OnUserAddedToChat
    {
        add => Updates.OnUserAddedToChat += value;
        remove => Updates.OnUserAddedToChat -= value;
    }

    /// <inheritdoc/>
    public event Func<UserRemovedFromChatUpdate, IMaxBotClient, Task>? OnUserRemovedFromChat
    {
        add => Updates.OnUserRemovedFromChat += value;
        remove => Updates.OnUserRemovedFromChat -= value;
    }

    /// <inheritdoc/>
    public event Func<DialogClearedUpdate, IMaxBotClient, Task>? OnDialogCleared
    {
        add => Updates.OnDialogCleared += value;
        remove => Updates.OnDialogCleared -= value;
    }

    /// <inheritdoc/>
    public event Func<DialogMutedUpdate, IMaxBotClient, Task>? OnDialogMuted
    {
        add => Updates.OnDialogMuted += value;
        remove => Updates.OnDialogMuted -= value;
    }

    /// <inheritdoc/>
    public event Func<DialogUnmutedUpdate, IMaxBotClient, Task>? OnDialogUnmuted
    {
        add => Updates.OnDialogUnmuted += value;
        remove => Updates.OnDialogUnmuted -= value;
    }

    /// <inheritdoc/>
    public event Func<DialogRemovedUpdate, IMaxBotClient, Task>? OnDialogRemoved
    {
        add => Updates.OnDialogRemoved += value;
        remove => Updates.OnDialogRemoved -= value;
    }

    /// <inheritdoc/>
    public event Func<CommentCreatedUpdate, IMaxBotClient, Task>? OnCommentCreated
    {
        add => Updates.OnCommentCreated += value;
        remove => Updates.OnCommentCreated -= value;
    }

    /// <inheritdoc/>
    public event Func<CommentEditedUpdate, IMaxBotClient, Task>? OnCommentEdited
    {
        add => Updates.OnCommentEdited += value;
        remove => Updates.OnCommentEdited -= value;
    }

    /// <inheritdoc/>
    public event Func<CommentRemovedUpdate, IMaxBotClient, Task>? OnCommentRemoved
    {
        add => Updates.OnCommentRemoved += value;
        remove => Updates.OnCommentRemoved -= value;
    }

    /// <inheritdoc/>
    public event Func<BotAdminPermissionsChangedUpdate, IMaxBotClient, Task>? OnBotAdminPermissionsChanged
    {
        add => Updates.OnBotAdminPermissionsChanged += value;
        remove => Updates.OnBotAdminPermissionsChanged -= value;
    }

    /// <inheritdoc/>
    public event Func<Exception, Task>? OnPollingError
    {
        add => Updates.OnPollingError += value;
        remove => Updates.OnPollingError -= value;
    }

    /// <inheritdoc/>
    public AttachmentRetryOptions AttachmentRetryOptions => _transport.AttachmentRetryOptions;

    /// <summary>
    /// Создать клиента с пользовательской реализацией сетевого транспорта <see cref="IMaxApiTransport"/>.
    /// Позволяет подменять сетевой слой для мок-тестирования или внедрения интерцепторов/логгеров.
    /// </summary>
    /// <param name="transport">Реализация сетевого транспорта API MAX.</param>
    public MaxBotClient(IMaxApiTransport transport)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        InitModules();
    }

    /// <summary>
    /// Создать клиента с внешним <see cref="HttpClient"/> (рекомендуется при использовании DI).
    /// </summary>
    public MaxBotClient(
        string token,
        HttpClient httpClient,
        AttachmentRetryOptions? attachmentRetryOptions = null)
        : this(new MaxBotCore(token, httpClient, attachmentRetryOptions))
    {
    }

    /// <summary>
    /// Создать клиента со встроенным <see cref="HttpClient"/>.
    /// </summary>
    public MaxBotClient(
        string token,
        int timeoutSeconds = 30,
        AttachmentRetryOptions? attachmentRetryOptions = null)
        : this(new MaxBotCore(token, timeoutSeconds, attachmentRetryOptions))
    {
    }

    private void InitModules()
    {
        Messages = new MessagesModule(_transport);
        Chats = new ChatsModule(_transport);
        Members = new MembersModule(_transport);
        Attachments = new AttachmentsModule(_transport);
        Updates = new UpdatesModule(_transport, this);
        Subscriptions = new SubscriptionsModule(_transport);
        Comments = new CommentsModule(_transport);
    }

    /// <inheritdoc/>
    public Task<User> GetMeAsync(CancellationToken cancellationToken = default)
        => _transport.SendRequestAsync<User>(HttpMethod.Get, "/me", null, cancellationToken);

    /// <inheritdoc/>
    public Task<BotInfo> GetMyInfoAsync(CancellationToken cancellationToken = default)
        => _transport.SendRequestAsync<BotInfo>(HttpMethod.Get, "/me", null, cancellationToken);

    /// <inheritdoc/>
    public Task<BotCommandsInfo> EditMyCommandsAsync(List<BotCommand> commands, CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(commands);
        return EditMyCommandsAsync(new BotCommandsPatch { Commands = commands }, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<BotCommandsInfo> EditMyCommandsAsync(BotCommandsPatch patch, CancellationToken cancellationToken = default)
    {
        ArgumentNullException_ThrowIfNull(patch);
        return _transport.SendRequestAsync<BotCommandsInfo>(HttpMethod_Patch, "/me/commands", patch, cancellationToken);
    }

    /// <inheritdoc/>
    public void StartPolling(
        Func<Update, IMaxBotClient, Task>? callback = null,
        int? limit = 100,
        int? timeout = 30,
        long? marker = null,
        List<string>? types = null,
        bool parallelProcessing = true)
    {
        lock (_pollLock)
        {
            // Игнорируем повторный вызов, если polling уже запущен
            if (_pollCts != null && !_pollCts.IsCancellationRequested)
                return;

            _pollCts = new CancellationTokenSource();
            var token = _pollCts.Token;

            _pollTask = Task.Run(async () =>
            {
                try
                {
                    await Updates.PollUpdatesWithCallback(callback, limit, timeout, marker, types, parallelProcessing, token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    // Штатная остановка polling — не требует обработки
                }
            }, token);
        }
    }

    /// <inheritdoc/>
    public Task DispatchUpdateAsync(Update update) => Updates.DispatchUpdateAsync(update);

    /// <inheritdoc/>
    public void StopPolling()
    {
        lock (_pollLock)
        {
            if (_pollCts == null)
                return;

            _pollCts.Cancel();
            _pollCts.Dispose();
            _pollCts = null;
            _pollTask = null;
        }
    }

    /// <inheritdoc/>
    public async Task StopPollingAsync(CancellationToken cancellationToken = default)
    {
        Task? pollTaskToWait;
        lock (_pollLock)
        {
            if (_pollCts == null)
                return;

            _pollCts.Cancel();
            pollTaskToWait = _pollTask;
        }

        if (pollTaskToWait != null)
        {
            try
            {
                await Task.WhenAny(pollTaskToWait, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Игнорируем отмену ожидания
            }
        }

        lock (_pollLock)
        {
            _pollCts?.Dispose();
            _pollCts = null;
            _pollTask = null;
        }
    }

    /// <summary>
    /// Асинхронно освобождает ресурсы клиента: штатно останавливает polling с ожиданием и закрывает сетевой транспорт.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await StopPollingAsync().ConfigureAwait(false);
        _transport.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Освобождает ресурсы клиента: останавливает polling и при необходимости закрывает HttpClient.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        StopPolling();
        _transport.Dispose();

        GC.SuppressFinalize(this);
    }
}

