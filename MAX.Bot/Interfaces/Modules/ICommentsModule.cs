using MAX.Bot.Interfaces.Models.Comments;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Response;

namespace MAX.Bot.Interfaces;

/// <summary>
/// Методы для работы с комментариями к сообщениям каналов: получение, отправка, редактирование и удаление.
/// </summary>
public interface ICommentsModule
{
    /// <summary>
    /// Получить список комментариев к сообщению канала с поддержкой пагинации.
    /// </summary>
    /// <param name="request">Параметры запроса получения комментариев.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Список комментариев к сообщению канала.</returns>
    Task<CommentMessageList> GetCommentsAsync(
        GetCommentsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить отдельный комментарий по его идентификатору.
    /// </summary>
    /// <param name="messageId">Идентификатор комментируемого сообщения (mid).</param>
    /// <param name="commentId">Идентификатор комментария (mid).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Информация о комментарии.</returns>
    Task<CommentMessage> GetCommentByIdAsync(
        string messageId,
        string commentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправить комментарий к сообщению канала.
    /// </summary>
    /// <param name="request">Параметры запроса отправки комментария.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат отправки комментария с созданным объектом комментария.</returns>
    Task<SendCommentResult> SendCommentAsync(
        SendCommentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправить текстовый комментарий к сообщению канала.
    /// </summary>
    /// <param name="messageId">Идентификатор комментируемого сообщения (mid).</param>
    /// <param name="text">Текст комментария.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат отправки комментария с созданным объектом комментария.</returns>
    Task<SendCommentResult> SendCommentAsync(
        string messageId,
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отредактировать комментарий к сообщению канала.
    /// </summary>
    /// <param name="request">Параметры запроса редактирования комментария.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Стандартный ответ с результатом выполнения операции.</returns>
    Task<BaseResponse> EditCommentAsync(
        EditCommentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отредактировать текст комментария к сообщению канала.
    /// </summary>
    /// <param name="messageId">Идентификатор комментируемого сообщения (mid).</param>
    /// <param name="commentId">Идентификатор редактируемого комментария (mid).</param>
    /// <param name="text">Новый текст комментария.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Стандартный ответ с результатом выполнения операции.</returns>
    Task<BaseResponse> EditCommentAsync(
        string messageId,
        string commentId,
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить комментарий к сообщению канала.
    /// </summary>
    /// <param name="request">Параметры запроса удаления комментария.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Стандартный ответ с результатом выполнения операции.</returns>
    Task<BaseResponse> DeleteCommentAsync(
        DeleteCommentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить комментарий к сообщению канала по его идентификаторам.
    /// </summary>
    /// <param name="messageId">Идентификатор комментируемого сообщения (mid).</param>
    /// <param name="commentId">Идентификатор удаляемого комментария (mid).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Стандартный ответ с результатом выполнения операции.</returns>
    Task<BaseResponse> DeleteCommentAsync(
        string messageId,
        string commentId,
        CancellationToken cancellationToken = default);
}
