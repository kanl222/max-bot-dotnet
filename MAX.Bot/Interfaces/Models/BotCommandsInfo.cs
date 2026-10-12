using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models;

/// <summary>
/// Информация о командах бота.
/// </summary>
public record BotCommandsInfo
{
    /// <summary>
    /// Список команд, поддерживаемых ботом.
    /// </summary>
    [JsonPropertyName("commands")]
    public List<BotCommand> Commands { get; set; } = new();
}
