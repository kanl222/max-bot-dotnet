using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Response;

namespace MAX.Bot.Interfaces;

/// <summary>
/// Методы для управления чатами.
/// </summary>
public interface IChatsModule
{
    /// <summary>
    /// Получить список чатов.
    /// </summary>
    Task<GetChatsResponse> GetChatsAsync(
        GetChatsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить информацию о групповом чате по идентификатору.
    /// </summary>
    Task<Chat> GetChatByIdAsync(long chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Изменить информацию о групповом чате.
    /// </summary>
    Task<Chat> UpdateChatAsync(
        long chatId,
        UpdateChatRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить групповой чат для всех участников.
    /// </summary>
    Task<BaseResponse> DeleteChatAsync(long chatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправить действие бота в групповой чат.
    /// </summary>
    Task<BaseResponse> SendChatActionAsync(
        long chatId,
        SendChatActionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить закрепленное сообщение в групповом чате.
    /// </summary>
    Task<GetChatPinnedMessageResponse> GetChatPinnedMessageAsync(
        long chatId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Закрепить сообщение в групповом чате.
    /// </summary>
    Task<BaseResponse> PinChatMessageAsync(
        long chatId,
        PinChatMessageRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить закрепленное сообщение в групповом чате.
    /// </summary>
    Task<BaseResponse> UnpinChatMessageAsync(long chatId, CancellationToken cancellationToken = default);
}
