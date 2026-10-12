using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Attachment;

namespace MAX.Bot.Interfaces;

/// <summary>
/// Фасад клиента для работы с API MAX Bot.
/// Доступ ко всем методам — через свойства-модули.
/// </summary>
public interface IMaxBotClient : IDisposable, IAsyncDisposable
{
    /// <summary>Методы для работы с сообщениями.</summary>
    IMessagesModule Messages { get; }

    /// <summary>Методы для управления чатами.</summary>
    IChatsModule Chats { get; }

    /// <summary>Методы для управления участниками и администраторами.</summary>
    IMembersModule Members { get; }

    /// <summary>Методы для работы с вложениями.</summary>
    IAttachmentsModule Attachments { get; }

    /// <summary>Методы для получения обновлений (long polling).</summary>
    IUpdatesModule Updates { get; }

    /// <summary>Методы для управления Webhook-подписками.</summary>
    ISubscriptionsModule Subscriptions { get; }

    /// <summary>Методы для работы с комментариями к сообщениям каналов.</summary>
    ICommentsModule Comments { get; }

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

    /// <summary>Вызывается при возникновении ошибки во время долгосрочного опроса.</summary>
    event Func<Exception, Task>? OnPollingError;

    /// <summary>
    /// Получить информацию о текущем боте.
    /// </summary>
    Task<User> GetMeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить полную информацию о текущем боте, включая список зарегистрированных команд.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Информация о боте со списком команд.</returns>
    Task<BotInfo> GetMyInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Установить или изменить список команд бота.
    /// </summary>
    /// <param name="commands">Список поддерживаемых команд бота (до 32 команд).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Обновлённый список команд бота.</returns>
    Task<BotCommandsInfo> EditMyCommandsAsync(List<BotCommand> commands, CancellationToken cancellationToken = default);

    /// <summary>
    /// Установить или изменить список команд бота с помощью объекта <see cref="BotCommandsPatch"/>.
    /// </summary>
    /// <param name="patch">Объект с новыми командами бота.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Обновлённый список команд бота.</returns>
    Task<BotCommandsInfo> EditMyCommandsAsync(BotCommandsPatch patch, CancellationToken cancellationToken = default);

    /// <summary>
    /// Запустить фоновый long-polling цикл.
    /// Метод немедленно возвращается; опрос идёт в фоне.
    /// Повторный вызов без предварительного <see cref="StopPolling"/> — игнорируется.
    /// </summary>
    /// <param name="callback">Необязательный дополнительный обработчик каждого входящего обновления.</param>
    /// <param name="limit">Максимальное число обновлений за один запрос (1–100).</param>
    /// <param name="timeout">Таймаут long-polling запроса в секундах.</param>
    /// <param name="marker">Начальная позиция в очереди обновлений (null — с последнего).</param>
    /// <param name="parallelProcessing">
    /// Выполнять ли обработку каждого обновления в отдельной фоновой задаче (true по умолчанию).
    /// Предотвращает Head-of-Line Blocking, когда медленная обработка одного события блокирует всю очередь.
    /// </param>
    void StartPolling(
        Func<Update, IMaxBotClient, Task>? callback = null,
        int? limit = 100,
        int? timeout = 30,
        long? marker = null,
        List<string>? types = null,
        bool parallelProcessing = true);

    /// <summary>
    /// Отправить принятое обновление всем зарегистрированным обработчикам событий.
    /// </summary>
    Task DispatchUpdateAsync(Update update);

    /// <summary>
    /// Остановить фоновый long-polling цикл, запущенный через <see cref="StartPolling"/>.
    /// Безопасно вызывать, даже если polling не был запущен.
    /// </summary>
    void StopPolling();

    /// <summary>
    /// Асинхронно остановить фоновый long-polling цикл с ожиданием завершения текущего запроса к API.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены ожидания остановки.</param>
    Task StopPollingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Настройки повторных попыток при ошибке «вложение ещё обрабатывается».
    /// </summary>
    AttachmentRetryOptions AttachmentRetryOptions { get; }
}
