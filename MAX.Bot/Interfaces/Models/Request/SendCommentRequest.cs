using System.Text.Json.Serialization;
using MAX.Bot.Interfaces.Models.Comments;

namespace MAX.Bot.Interfaces.Models.Request;

/// <summary>
/// Запрос на отправку комментария к сообщению канала.
/// </summary>
public record SendCommentRequest
{
    /// <summary>
    /// Идентификатор комментируемого сообщения (mid).
    /// </summary>
    [JsonIgnore]
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Тело нового комментария.
    /// </summary>
    [JsonPropertyName("body")]
    public NewCommentBody Body { get; set; } = new();

    /// <summary>
    /// Если true, сервер не будет генерировать предпросмотр медиа для ссылок в тексте комментария.
    /// </summary>
    [JsonIgnore]
    public bool? DisableLinkPreview { get; set; }

    /// <summary>
    /// Проверить параметры запроса на отправку комментария.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(MessageId))
            throw new ArgumentException("MessageId не должен быть пустым.", nameof(MessageId));

        if (Body == null)
            throw new ArgumentNullException(nameof(Body), "Тело комментария не должно быть null.");

        Body.Validate();
    }
}
