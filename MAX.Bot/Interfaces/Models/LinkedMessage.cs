using System.Text.Json.Serialization;
using MAX.Bot.Interfaces.Models.Request.Message.Link;

namespace MAX.Bot.Interfaces.Models;

/// <summary>
/// Пересланное или ответное сообщение (связанное сообщение).
/// </summary>
public record LinkedMessage
{
    /// <summary>
    /// Тип связи сообщения (пересланное или ответ).
    /// </summary>
    [JsonPropertyName("type")]
    public MessageLinkType Type { get; set; }

    /// <summary>
    /// Пользователь, отправивший исходное сообщение. Может быть null, если отправлено от имени канала.
    /// </summary>
    [JsonPropertyName("sender")]
    public User? Sender { get; set; }

    /// <summary>
    /// Идентификатор чата, в котором сообщение было изначально опубликовано.
    /// </summary>
    [JsonPropertyName("chat_id")]
    public long? ChatId { get; set; }

    /// <summary>
    /// Тело исходного сообщения.
    /// </summary>
    [JsonPropertyName("message")]
    public MessageBody Message { get; set; } = null!;
}
