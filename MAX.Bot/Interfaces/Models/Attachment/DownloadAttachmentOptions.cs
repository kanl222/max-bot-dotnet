using MAX.Bot.Interfaces.Models.Response;

namespace MAX.Bot.Interfaces.Models.Attachment;

/// <summary>
/// Параметры скачивания вложения. Повторные попытки наследуются из <see cref="AttachmentRetryOptions"/> (по умолчанию 4 попытки, пауза 2 секунды).
/// </summary>
public class DownloadAttachmentOptions : AttachmentRetryOptions
{
    /// <summary>
    /// Путь для сохранения файла на диск. Если не указан, содержимое возвращается в <see cref="AttachmentDownloadResult.Content"/>.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Качество видео при скачивании <see cref="Request.Message.Attachment.VideoAttachment"/>. По умолчанию — 480p.
    /// </summary>
    public VideoQuality VideoQuality { get; set; } = VideoQuality.Mp4_480;

    /// <summary>
    /// Расширение файла по умолчанию, если его не удалось определить из имени или URL.
    /// </summary>
    public string? DefaultExtension { get; set; }

    /// <summary>
    /// Максимальный размер вложения (в байтах) для скачивания в оперативную память, если <see cref="FilePath"/> не задан.
    /// По умолчанию — 50 МБ (52 428 800 байт). Предотвращает исчерпание оперативной памяти (OOM DoS).
    /// </summary>
    public long MaxInMemorySizeBytes { get; set; } = 50L * 1024 * 1024;

    /// <summary>
    /// Таймаут на операцию скачивания и чтение потока данных. По умолчанию — 60 секунд.
    /// Предотвращает зависание при медленной или зависшей передаче данных сервером.
    /// </summary>
    public TimeSpan DownloadTimeout { get; set; } = TimeSpan.FromSeconds(60);
}
