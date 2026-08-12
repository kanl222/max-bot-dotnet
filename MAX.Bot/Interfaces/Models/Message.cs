using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models;

/// <summary>
/// Модель сообщения
/// </summary>
public record Message
{
    /// <summary>
    /// Сообщение в чате
    /// </summary>
    [JsonPropertyName("sender")]
    public User? Sender { get; set; }

    /// <summary>
    /// Время создания сообщения в формате Unix-time
    /// </summary>
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    /// <summary>
    /// Содержимое сообщения. Текст + вложения. Может быть null, 
    /// если сообщение содержит только пересланное сообщение
    /// </summary>
    [JsonPropertyName("body")]
    public MessageBody? Body { get; set; }

    /// <summary>
    /// Получатель сообщения
    /// </summary>
    [JsonPropertyName("recipient")]
    public Recipient? Recipient { get; set; }

    /// <summary>
    /// Публичная ссылка на пост в канале. Отсутствует для диалогов и групповых чатов
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}