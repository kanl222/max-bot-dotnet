using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models;

/// <summary>
/// Модель тела сообщения
/// </summary>
public record MessageBody
{
    /// <summary>
    /// Уникальный ID сообщения
    /// </summary>
    [JsonPropertyName("mid")]
    public string? Mid { get; set; }

    /// <summary>
    /// Порядковый номер сообщения в чате
    /// </summary>
    [JsonPropertyName("seq")]
    public long? Seq { get; set; }

    /// <summary>
    /// Новый текст сообщения
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Разметка (форматирование) текста сообщения
    /// </summary>
    [JsonPropertyName("markup")]
    public List<Markup.MarkupElement>? Markup { get; set; }

    /// <summary>
    /// Вложения сообщения
    /// </summary>
    [JsonPropertyName("attachments")]
    public List<Request.Message.Attachment.Attachment>? Attachments { get; set; }
}