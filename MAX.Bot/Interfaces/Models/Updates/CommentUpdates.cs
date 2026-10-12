using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models;

/// <summary>
/// Обновление: создан новый комментарий к сообщению канала.
/// Бот получает это обновление, только если является администратором канала с правом <c>read_all_messages</c>.
/// </summary>
public record CommentCreatedUpdate : Update
{
    /// <summary>
    /// Инициализирует новый экземпляр обновления о создании комментария.
    /// </summary>
    public CommentCreatedUpdate()
    {
        UpdateType = UpdateTypes.CommentCreated;
    }

    /// <summary>
    /// Созданный комментарий.
    /// </summary>
    [JsonPropertyName("message")]
    public Message Message { get; set; } = null!;
}

/// <summary>
/// Обновление: комментарий к сообщению канала был отредактирован.
/// Бот получает это обновление, только если является администратором канала с правом <c>read_all_messages</c>.
/// </summary>
public record CommentEditedUpdate : Update
{
    /// <summary>
    /// Инициализирует новый экземпляр обновления о редактировании комментария.
    /// </summary>
    public CommentEditedUpdate()
    {
        UpdateType = UpdateTypes.CommentEdited;
    }

    /// <summary>
    /// Отредактированный комментарий.
    /// </summary>
    [JsonPropertyName("message")]
    public Message Message { get; set; } = null!;
}

/// <summary>
/// Обновление: комментарий к сообщению канала был удален.
/// Бот получает это обновление, только если является администратором канала с правом <c>read_all_messages</c>.
/// </summary>
public record CommentRemovedUpdate : Update
{
    /// <summary>
    /// Инициализирует новый экземпляр обновления об удалении комментария.
    /// </summary>
    public CommentRemovedUpdate()
    {
        UpdateType = UpdateTypes.CommentRemoved;
    }

    /// <summary>
    /// Идентификатор чата (канала), где был удален комментарий.
    /// </summary>
    [JsonPropertyName("chat_id")]
    public long ChatId { get; set; }

    /// <summary>
    /// Идентификатор удаленного комментария.
    /// </summary>
    [JsonPropertyName("message_id")]
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор комментируемого поста канала.
    /// </summary>
    [JsonPropertyName("post_id")]
    public string PostId { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор пользователя, удалившего комментарий.
    /// </summary>
    [JsonPropertyName("user_id")]
    public long UserId { get; set; }
}
