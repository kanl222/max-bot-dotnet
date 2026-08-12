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
    /// </summary>
    Task PollUpdatesWithCallback(
        Func<Update, IMaxBotClient, Task>? callback = null,
        int? limit = 100,
        int? timeout = 30,
        long? marker = null,
        List<string>? types = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправить принятое обновление всем зарегистрированным обработчикам событий.
    /// Полезно при работе через Webhook.
    /// </summary>
    Task DispatchUpdateAsync(Update update);
}
