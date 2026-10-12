using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models;

/// <summary>
/// Команда бота с именем и описанием.
/// </summary>
public record BotCommand
{
    /// <summary>
    /// Имя команды (от 1 до 32 символов, латинские строчные буквы, цифры и знак подчёркивания: ^[a-z0-9_]+$).
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание команды (до 128 символов).
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
