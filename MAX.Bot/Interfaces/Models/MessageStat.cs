using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models;

/// <summary>
/// Статистика сообщения в канале.
/// </summary>
public record MessageStat
{
    /// <summary>
    /// Количество просмотров сообщения.
    /// </summary>
    [JsonPropertyName("views")]
    public int Views { get; set; }
}
