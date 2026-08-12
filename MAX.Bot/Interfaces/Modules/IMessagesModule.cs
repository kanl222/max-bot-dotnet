using MAX.Bot.Interfaces.Models;
using MAX.Bot.Interfaces.Models.Attachment;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Request.Message;
using MAX.Bot.Interfaces.Models.Response;

namespace MAX.Bot.Interfaces;

/// <summary>
/// Методы для работы с сообщениями.
/// </summary>
public interface IMessagesModule
{
    /// <summary>
    /// Отправить сообщение. При отправке с upload-вложениями автоматически повторяет запрос,
    /// если API возвращает ошибку «вложение ещё обрабатывается».
    /// </summary>
    Task<SendMessageResponse> SendMessageAsync(
        SendMessageRequest request,
        AttachmentRetryOptions? retryOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ответить на callback после нажатия пользователем кнопки.
    /// </summary>
    Task<BaseResponse> AnswerCallbackAsync(
        AnswerCallbackRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить сообщения из чата.
    /// </summary>
    Task<GetMessagesResponse> GetMessagesAsync(
        GetMessagesRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить сообщение по идентификатору.
    /// </summary>
    Task<Message> GetMessageByIdAsync(
        string messageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отредактировать сообщение по идентификатору. При редактировании с upload-вложениями
    /// автоматически повторяет запрос, если API возвращает ошибку «вложение ещё обрабатывается».
    /// </summary>
    Task<BaseResponse> EditMessageByIdAsync(
        string messageId,
        SendMessageRequest messageRequest,
        AttachmentRetryOptions? retryOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить сообщение по идентификатору.
    /// </summary>
    Task<BaseResponse> DeleteMessageByIdAsync(
        string messageId,
        CancellationToken cancellationToken = default);
}
