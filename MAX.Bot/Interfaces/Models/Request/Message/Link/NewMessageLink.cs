using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models.Request.Message.Link;

public record NewMessageLink
{
    /// <summary>
    /// Enum: "forward" "reply"
    /// Тип ссылки сообщения
    /// </summary>
    [JsonPropertyName("type")]
    public MessageLinkType Type { get; set; }

    /// <summary>
    /// ID сообщения исходного сообщения
    /// </summary>
    [JsonPropertyName("mid")]
    public string MId { get; set; }
}