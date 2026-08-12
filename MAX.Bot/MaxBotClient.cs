using MAX.Bot.Interfaces;
using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Attachment;
using MAX.Bot.Modules;

namespace MAX.Bot;

/// <summary>
/// Фасад-клиент для API MAX Bot.
/// Все методы сгруппированы по модулям: Messages, Chats, Members, Attachments, Updates, Subscriptions.
/// </summary>
public class MaxBotClient : IMaxBotClient, IDisposable
{
    /// <summary>Базовый URL API MAX Bot.</summary>
    public const string BaseUrl = "https://platform-api2.max.ru";

    private readonly MaxBotCore _core;
    private readonly object _pollLock = new();
    private CancellationTokenSource? _pollCts;
    private Task? _pollTask;
    private bool _disposed;

    /// <inheritdoc/>
    public IMessagesModule Messages { get; private set; }

    /// <inheritdoc/>
    public IChatsModule Chats { get; private set; }

    /// <inheritdoc/>
    public IMembersModule Members { get; private set; }

    /// <inheritdoc/>
    public IAttachmentsModule Attachments { get; private set; }

    /// <inheritdoc/>
    public IUpdatesModule Updates { get; private set; }

    /// <inheritdoc/>
    public ISubscriptionsModule Subscriptions { get; private set; }

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
    public event Func<Exception, Task>? OnPollingError
    {
        add => Updates.OnPollingError += value;
        remove => Updates.OnPollingError -= value;
    }

    /// <inheritdoc/>
    public AttachmentRetryOptions AttachmentRetryOptions => _core.AttachmentRetryOptions;

    /// <summary>
    /// Создать клиента с внешним <see cref="HttpClient"/> (рекомендуется при использовании DI).
    /// </summary>
    public MaxBotClient(
        string token,
        HttpClient httpClient,
        AttachmentRetryOptions? attachmentRetryOptions = null)
    {
        _core = new MaxBotCore(token, httpClient, attachmentRetryOptions);
        InitModules();
    }

    /// <summary>
    /// Создать клиента со встроенным <see cref="HttpClient"/>.
    /// </summary>
    public MaxBotClient(
        string token,
        int timeoutSeconds = 30,
        AttachmentRetryOptions? attachmentRetryOptions = null)
    {
        _core = new MaxBotCore(token, timeoutSeconds, attachmentRetryOptions);
        InitModules();
    }

    private void InitModules()
    {
        Messages = new MessagesModule(_core);
        Chats = new ChatsModule(_core);
        Members = new MembersModule(_core);
        Attachments = new AttachmentsModule(_core);
        Updates = new UpdatesModule(_core, this);
        Subscriptions = new SubscriptionsModule(_core);
    }

    /// <inheritdoc/>
    public Task<User> GetMeAsync(CancellationToken cancellationToken = default)
        => _core.SendRequestAsync<User>(HttpMethod.Get, "/me", null, cancellationToken);

    /// <inheritdoc/>
    public void StartPolling(
        Func<Update, IMaxBotClient, Task>? callback = null,
        int? limit = 100,
        int? timeout = 30,
        long? marker = null,
        List<string>? types = null)
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
                    await Updates.PollUpdatesWithCallback(callback, limit, timeout, marker, types, token);
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

    /// <summary>
    /// Освобождает ресурсы клиента: останавливает polling и при необходимости закрывает HttpClient.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        StopPolling();
        _core.Dispose();

        GC.SuppressFinalize(this);
    }
}
