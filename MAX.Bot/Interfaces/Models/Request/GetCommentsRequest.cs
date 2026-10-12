using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models.Request;

/// <summary>
/// Запрос на получение комментариев к сообщению канала.
/// </summary>
public record GetCommentsRequest
{
    /// <summary>
    /// Идентификатор комментируемого сообщения (mid).
    /// </summary>
    [JsonIgnore]
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Список идентификаторов комментариев для выборки.
    /// </summary>
    [JsonPropertyName("comment_ids")]
    public List<string>? CommentIds { get; set; }

    /// <summary>
    /// Комментарии до указанной временной метки (Unix timestamp).
    /// </summary>
    [JsonPropertyName("before")]
    public long? Before { get; set; }

    /// <summary>
    /// Комментарии после указанной временной метки (Unix timestamp).
    /// </summary>
    [JsonPropertyName("after")]
    public long? After { get; set; }

    /// <summary>
    /// Максимальное количество комментариев в ответе (от 1 до 100, по умолчанию 50).
    /// </summary>
    [JsonPropertyName("count")]
    public int? Count { get; set; }

    /// <summary>
    /// Проверить параметры запроса комментариев.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(MessageId))
            throw new ArgumentException("MessageId не должен быть пустым.", nameof(MessageId));

        if (CommentIds != null && CommentIds.Exists(string.IsNullOrWhiteSpace))
            throw new ArgumentException("comment_ids не должен содержать пустые идентификаторы.", nameof(CommentIds));

        if (Count is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(Count), Count, "count должен быть в диапазоне от 1 до 100.");

        if (After.HasValue && Before.HasValue && After.Value > Before.Value)
            throw new ArgumentException("after не должен быть больше before.");
    }
}
