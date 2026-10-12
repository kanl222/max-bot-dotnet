using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models.Comments;

/// <summary>
/// Результат отправки комментария.
/// </summary>
public record SendCommentResult
{
    /// <summary>
    /// Созданный комментарий.
    /// </summary>
    [JsonPropertyName("message")]
    public CommentMessage Message { get; set; } = null!;
}
