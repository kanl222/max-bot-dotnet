using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models;

/// <summary>
/// Информация о боте, включая список команд и статус.
/// </summary>
public record BotInfo : User
{
    /// <summary>
    /// Команды, поддерживаемые ботом (до 32 команд).
    /// </summary>
    [JsonPropertyName("commands")]
    public List<BotCommand>? Commands { get; set; }
}
