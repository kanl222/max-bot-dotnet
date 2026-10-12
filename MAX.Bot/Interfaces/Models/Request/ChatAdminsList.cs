using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models.Request;

/// <summary>
/// Список администраторов чата с их правами (соответствует схеме ChatAdminsList в OpenAPI).
/// </summary>
public record ChatAdminsList
{
    /// <summary>
    /// Список пользователей с их правами администратора.
    /// </summary>
    [JsonPropertyName("admins")]
    public List<ChatAdmin> Admins { get; set; } = new();
}
