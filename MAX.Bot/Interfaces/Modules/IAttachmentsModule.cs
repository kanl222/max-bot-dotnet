using MAX.Bot.Interfaces.Models.Attachment;
using MAX.Bot.Interfaces.Models.Request;
using MAX.Bot.Interfaces.Models.Request.Message.Attachment;
using MAX.Bot.Interfaces.Models.Response;

namespace MAX.Bot.Interfaces;

/// <summary>
/// Методы для работы с вложениями: загрузка, скачивание, видео.
/// </summary>
public interface IAttachmentsModule
{
    /// <summary>
    /// Получить информацию о видео по токену вложения.
    /// Автоматически повторяет запрос, если видео ещё обрабатывается.
    /// </summary>
    Task<VideoInfoResponse> GetVideoAsync(
        string videoToken,
        AttachmentRetryOptions? retryOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает URL для скачивания вложения или null, если оно ещё не готово.
    /// </summary>
    Task<string?> TryGetAttachmentDownloadUrlAsync(
        Attachment attachment,
        VideoQuality videoQuality = VideoQuality.Mp4_480,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Скачать вложение: получить содержимое в память или сохранить на диск.
    /// </summary>
    Task<AttachmentDownloadResult> DownloadAttachmentAsync(
        Attachment attachment,
        DownloadAttachmentOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Загрузить файл и получить токен вложения.
    /// </summary>
    Task<string> UploadAsync(UploadRequest request, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="UploadAsync"/>
    [Obsolete("Используйте UploadAsync вместо UploadsAsync.")]
    Task<string> UploadsAsync(UploadRequest request, CancellationToken cancellationToken = default);
}
