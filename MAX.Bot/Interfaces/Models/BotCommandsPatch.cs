using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models;

/// <summary>
/// Объект запроса для обновления списка команд бота.
/// </summary>
public record BotCommandsPatch
{
    /// <summary>
    /// Список команд, поддерживаемых ботом (до 32 команд).
    /// </summary>
    [JsonPropertyName("commands")]
    public List<BotCommand> Commands { get; set; } = new();
}
