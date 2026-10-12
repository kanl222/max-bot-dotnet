using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models.Comments;

/// <summary>
/// Сообщение-комментарий к посту в канале. В отличие от Message, не содержит публичного URL и вложений.
/// </summary>
public record CommentMessage
{
    /// <summary>
    /// Пользователь, оставивший комментарий. Может быть null, если комментарий отправлен от имени канала.
    /// </summary>
    [JsonPropertyName("sender")]
    public User? Sender { get; set; }

    /// <summary>
    /// Время создания комментария в формате Unix-time.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    /// <summary>
    /// Тело комментария.
    /// </summary>
    [JsonPropertyName("body")]
    public CommentMessageBody Body { get; set; } = null!;

    /// <summary>
    /// Получатель сообщения (для комментариев — канал).
    /// </summary>
    [JsonPropertyName("recipient")]
    public Recipient Recipient { get; set; } = null!;

    /// <summary>
    /// Связанный комментарий (пересланный или ответ).
    /// </summary>
    [JsonPropertyName("link")]
    public CommentLinkedMessage? Link { get; set; }

    /// <summary>
    /// Статистика комментария.
    /// </summary>
    [JsonPropertyName("stat")]
    public MessageStat? Stat { get; set; }
}
