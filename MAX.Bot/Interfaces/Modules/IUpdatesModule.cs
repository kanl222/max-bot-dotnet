using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Response;

namespace MAX.Bot.Interfaces;

/// <summary>
/// Методы для получения обновлений (long polling).
/// </summary>
public interface IUpdatesModule
{
    /// <summary>Вызывается при получении любого входящего обновления.</summary>
    event Func<Update, IMaxBotClient, Task>? OnUpdate;

    /// <summary>Вызывается при получении нового сообщения.</summary>
    event Func<MessageCreatedUpdate, IMaxBotClient, Task>? OnMessageCreated;

    /// <summary>Вызывается при редактировании сообщения.</summary>
    event Func<MessageEditedUpdate, IMaxBotClient, Task>? OnMessageEdited;

    /// <summary>Вызывается при удалении сообщения.</summary>
    event Func<MessageRemovedUpdate, IMaxBotClient, Task>? OnMessageRemoved;

    /// <summary>Вызывается при нажатии кнопки inline-клавиатуры (callback).</summary>
    event Func<MessageCallbackUpdate, IMaxBotClient, Task>? OnMessageCallback;

    /// <summary>Вызывается при добавлении бота в чат или канал.</summary>
    event Func<BotAddedToChatUpdate, IMaxBotClient, Task>? OnBotAddedToChat;

    /// <summary>Вызывается при запуске или возобновлении общения с ботом.</summary>
    event Func<BotStartedUpdate, IMaxBotClient, Task>? OnBotStarted;

    /// <summary>Вызывается при остановке бота пользователем.</summary>
    event Func<BotStoppedUpdate, IMaxBotClient, Task>? OnBotStopped;

    /// <summary>Вызывается при удалении бота из чата или канала.</summary>
    event Func<BotRemovedFromChatUpdate, IMaxBotClient, Task>? OnBotRemovedFromChat;

    /// <summary>Вызывается при изменении названия чата или канала.</summary>
    event Func<ChatTitleChangedUpdate, IMaxBotClient, Task>? OnChatTitleChanged;

    /// <summary>Вызывается при добавлении нового пользователя в чат или канал.</summary>
    event Func<UserAddedToChatUpdate, IMaxBotClient, Task>? OnUserAddedToChat;

    /// <summary>Вызывается при удалении пользователя из чата или канала.</summary>
    event Func<UserRemovedFromChatUpdate, IMaxBotClient, Task>? OnUserRemovedFromChat;

    /// <summary>Вызывается при очистке истории диалога пользователем.</summary>
    event Func<DialogClearedUpdate, IMaxBotClient, Task>? OnDialogCleared;

    /// <summary>Вызывается при отключении уведомлений в диалоге.</summary>
    event Func<DialogMutedUpdate, IMaxBotClient, Task>? OnDialogMuted;

    /// <summary>Вызывается при включении уведомлений в диалоге.</summary>
    event Func<DialogUnmutedUpdate, IMaxBotClient, Task>? OnDialogUnmuted;

    /// <summary>Вызывается при удалении диалога пользователем.</summary>
    event Func<DialogRemovedUpdate, IMaxBotClient, Task>? OnDialogRemoved;

    /// <summary>Вызывается при создании нового комментария к сообщению канала.</summary>
    event Func<CommentCreatedUpdate, IMaxBotClient, Task>? OnCommentCreated;

    /// <summary>Вызывается при редактировании комментария к сообщению канала.</summary>
    event Func<CommentEditedUpdate, IMaxBotClient, Task>? OnCommentEdited;

    /// <summary>Вызывается при удалении комментария к сообщению канала.</summary>
    event Func<CommentRemovedUpdate, IMaxBotClient, Task>? OnCommentRemoved;

    /// <summary>Вызывается при изменении прав администратора у бота в чате или канале.</summary>
    event Func<BotAdminPermissionsChangedUpdate, IMaxBotClient, Task>? OnBotAdminPermissionsChanged;

    /// <summary>
    /// Вызывается при возникновении ошибки во время долгосрочного опроса.
    /// Если обработчик не задан, ошибки игнорируются с паузой между попытками.
    /// </summary>
    event Func<Exception, Task>? OnPollingError;

    /// <summary>
    /// Получить обновления (события).
    /// </summary>
    Task<GetUpdatesResponse> GetUpdatesAsync(
        GetUpdatesRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Долгосрочный опрос обновлений с автоматической отправкой событий подписчикам.
    /// По умолчанию обработка обновлений выполняется параллельно для предотвращения взаимных блокировок.
    /// </summary>
    /// <param name="callback">Необязательный дополнительный обработчик каждого входящего обновления.</param>
    /// <param name="limit">Максимальное число обновлений за один запрос (1–100).</param>
    /// <param name="timeout">Таймаут long-polling запроса в секундах.</param>
    /// <param name="marker">Начальная позиция в очереди обновлений (null — с последнего).</param>
    /// <param name="types">Фильтр типов обновлений (null — все).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task PollUpdatesWithCallback(
        Func<Update, IMaxBotClient, Task>? callback = null,
        int? limit = 100,
        int? timeout = 30,
        long? marker = null,
        List<string>? types = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Долгосрочный опрос обновлений с явным указанием режима параллелизма.
    /// </summary>
    /// <param name="callback">Необязательный дополнительный обработчик каждого входящего обновления.</param>
    /// <param name="limit">Максимальное число обновлений за один запрос (1–100).</param>
    /// <param name="timeout">Таймаут long-polling запроса в секундах.</param>
    /// <param name="marker">Начальная позиция в очереди обновлений (null — с последнего).</param>
    /// <param name="types">Фильтр типов обновлений (null — все).</param>
    /// <param name="parallelProcessing">
    /// Выполнять ли обработку каждого обновления в отдельной фоновой задаче (true).
    /// При значении false обработка выполняется строго последовательно.
    /// </param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task PollUpdatesWithCallback(
        Func<Update, IMaxBotClient, Task>? callback,
        int? limit,
        int? timeout,
        long? marker,
        List<string>? types,
        bool parallelProcessing,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправить принятое обновление всем зарегистрированным обработчикам событий.
    /// Полезно при работе через Webhook.
    /// </summary>
    Task DispatchUpdateAsync(Update update);
}
