using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models.Request;

/// <summary>
/// Запрос на удаление комментария к сообщению канала.
/// </summary>
public record DeleteCommentRequest
{
    /// <summary>
    /// Идентификатор комментируемого сообщения (mid).
    /// </summary>
    [JsonIgnore]
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор удаляемого комментария (mid).
    /// </summary>
    [JsonIgnore]
    public string CommentId { get; set; } = string.Empty;

    /// <summary>
    /// Проверить параметры запроса на удаление комментария.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(MessageId))
            throw new ArgumentException("MessageId не должен быть пустым.", nameof(MessageId));

        if (string.IsNullOrWhiteSpace(CommentId))
            throw new ArgumentException("CommentId не должен быть пустым.", nameof(CommentId));
    }
}
