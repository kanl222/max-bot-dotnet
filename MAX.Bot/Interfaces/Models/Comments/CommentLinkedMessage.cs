using System.Text.Json.Serialization;
using MAX.Bot.Interfaces.Models.Request.Message.Link;

namespace MAX.Bot.Interfaces.Models.Comments;

/// <summary>
/// Пересланный комментарий или ответ на комментарий.
/// </summary>
public record CommentLinkedMessage
{
    /// <summary>
    /// Тип связи комментария (пересланный или ответ).
    /// </summary>
    [JsonPropertyName("type")]
    public MessageLinkType Type { get; set; }

    /// <summary>
    /// Пользователь, отправивший исходный комментарий. Может быть null, если комментарий отправлен от имени канала.
    /// </summary>
    [JsonPropertyName("sender")]
    public User? Sender { get; set; }

    /// <summary>
    /// Идентификатор чата, в котором был опубликован исходный комментарий.
    /// </summary>
    [JsonPropertyName("chat_id")]
    public long? ChatId { get; set; }

    /// <summary>
    /// Тело исходного комментария.
    /// </summary>
    [JsonPropertyName("message")]
    public CommentMessageBody Message { get; set; } = null!;
}
