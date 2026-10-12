using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models;

/// <summary>
/// Подробная информация о причине, по которой пользователей не удалось добавить в чат.
/// </summary>
public record FailedUserDetails
{
    /// <summary>
    /// Код ошибки (например, add.participant.privacy или add.participant.not.found).
    /// </summary>
    [JsonPropertyName("error_code")]
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Список ID пользователей, которых не удалось добавить.
    /// </summary>
    [JsonPropertyName("user_ids")]
    public List<long> UserIds { get; set; } = new();
}
