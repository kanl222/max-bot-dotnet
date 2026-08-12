namespace MAX.Bot.Interfaces.Models.Request.Message.Attachment.Payloads;

/// <summary>
/// Полезная нагрузка для вложения "Поделиться"
/// </summary>
public class SharePayload : AttachmentPayload
{
    /// <summary>
    /// URL для "Поделиться"
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Токен для "Поделиться"
    /// </summary>
    [JsonPropertyName("token")]
    public string? Token { get; set; }
}