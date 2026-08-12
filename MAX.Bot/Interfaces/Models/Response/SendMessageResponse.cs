using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models.Response;

/// <summary>
/// Модель ответа от АПИ на отправку сообщения
/// </summary>
public record SendMessageResponse
{
    /// <summary>
    /// Сообщение в чате
    /// </summary>
    [JsonPropertyName("message")]
    public Message? Message { get; set; }
}
