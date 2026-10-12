using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models.Comments;

/// <summary>
/// Список комментариев к сообщению канала с поддержкой пагинации.
/// </summary>
public record CommentMessageList
{
    /// <summary>
    /// Список комментариев к сообщению.
    /// </summary>
    [JsonPropertyName("messages")]
    public List<CommentMessage> Messages { get; set; } = new();
}
