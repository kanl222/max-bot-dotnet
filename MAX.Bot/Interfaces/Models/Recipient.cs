using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models;

/// <summary>
/// Получатель сообщения (чат, канал или личный диалог).
/// </summary>
public record Recipient
{
    /// <summary>
    /// Идентификатор чата или канала.
    /// </summary>
    [JsonPropertyName("chat_id")]
    public long ChatId { get; set; }

    /// <summary>
    /// Тип чата: dialog, chat, channel.
    /// </summary>
    [JsonPropertyName("chat_type")]
    public string? ChatType { get; set; }

    /// <summary>
    /// Идентификатор пользователя (для личных диалогов).
    /// </summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; set; }
}
