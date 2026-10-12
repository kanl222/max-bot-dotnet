using System.Text.Json.Serialization;
using MAX.Bot.Interfaces.Models.Comments;

namespace MAX.Bot.Interfaces.Models.Request;

/// <summary>
/// Запрос на редактирование комментария к сообщению канала.
/// </summary>
public record EditCommentRequest
{
    /// <summary>
    /// Идентификатор комментируемого сообщения (mid).
    /// </summary>
    [JsonIgnore]
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор редактируемого комментария (mid).
    /// </summary>
    [JsonIgnore]
    public string CommentId { get; set; } = string.Empty;

    /// <summary>
    /// Обновленное тело комментария.
    /// </summary>
    [JsonPropertyName("body")]
    public NewCommentBody Body { get; set; } = new();

    /// <summary>
    /// Проверить параметры запроса на редактирование комментария.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(MessageId))
            throw new ArgumentException("MessageId не должен быть пустым.", nameof(MessageId));

        if (string.IsNullOrWhiteSpace(CommentId))
            throw new ArgumentException("CommentId не должен быть пустым.", nameof(CommentId));

        if (Body == null)
            throw new ArgumentNullException(nameof(Body), "Тело комментария не должно быть null.");

        Body.Validate();
    }
}
