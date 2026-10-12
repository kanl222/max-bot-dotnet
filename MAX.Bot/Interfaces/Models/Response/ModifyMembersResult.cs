using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models.Response;

/// <summary>
/// Результат запроса на модификацию списка участников чата.
/// </summary>
public record ModifyMembersResult : BaseResponse
{
    /// <summary>
    /// Список ID пользователей, которых не удалось добавить или удалить.
    /// </summary>
    [JsonPropertyName("failed_user_ids")]
    public List<long>? FailedUserIds { get; set; }

    /// <summary>
    /// Подробная информация об ошибках добавления участников.
    /// </summary>
    [JsonPropertyName("failed_user_details")]
    public List<FailedUserDetails>? FailedUserDetails { get; set; }
}
