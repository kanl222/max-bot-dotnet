using System.Text.Json.Serialization;
using MAX.Bot.Interfaces.Models.Markup;

namespace MAX.Bot.Interfaces.Models.Comments;

/// <summary>
/// Тело комментария к сообщению канала. В отличие от сообщений, комментарии не могут содержать вложений.
/// </summary>
public record CommentMessageBody
{
    /// <summary>
    /// Уникальный идентификатор комментария.
    /// </summary>
    [JsonPropertyName("mid")]
    public string Mid { get; set; } = string.Empty;

    /// <summary>
    /// Порядковый номер комментария в чате.
    /// </summary>
    [JsonPropertyName("seq")]
    public long Seq { get; set; }

    /// <summary>
    /// Текст комментария.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Разметка (форматирование) текста комментария.
    /// </summary>
    [JsonPropertyName("markup")]
    public List<MarkupElement>? Markup { get; set; }
}
