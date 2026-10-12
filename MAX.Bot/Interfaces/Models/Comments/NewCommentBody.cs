using System.Text.Json.Serialization;
using MAX.Bot.Interfaces.Models.Request.Message.Link;

namespace MAX.Bot.Interfaces.Models.Comments;

/// <summary>
/// Тело нового комментария для отправки или редактирования.
/// </summary>
public record NewCommentBody
{
    /// <summary>
    /// Текст комментария (до 4000 символов).
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Форматирование текста комментария.
    /// </summary>
    [JsonPropertyName("format")]
    public MessageFormat? Format { get; set; }

    /// <summary>
    /// Ссылка на сообщение или комментарий (ответ).
    /// </summary>
    [JsonPropertyName("link")]
    public NewMessageLink? Link { get; set; }

    /// <summary>
    /// Валидация параметров тела комментария.
    /// </summary>
    public void Validate()
    {
        if (Text is { Length: > 4000 })
            throw new ArgumentException("Текст комментария не должен превышать 4000 символов.", nameof(Text));
    }
}
