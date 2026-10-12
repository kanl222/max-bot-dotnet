using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models.Markup;

/// <summary>
/// Базовый класс для элементов разметки (форматирования) текста.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(StrongMarkup), "strong")]
[JsonDerivedType(typeof(EmphasizedMarkup), "emphasized")]
[JsonDerivedType(typeof(MonospacedMarkup), "monospaced")]
[JsonDerivedType(typeof(LinkMarkup), "link")]
[JsonDerivedType(typeof(StrikethroughMarkup), "strikethrough")]
[JsonDerivedType(typeof(UnderlineMarkup), "underline")]
[JsonDerivedType(typeof(HeadingMarkup), "heading")]
[JsonDerivedType(typeof(UserMentionMarkup), "user_mention")]
[JsonDerivedType(typeof(HighlightedMarkup), "highlighted")]
[JsonDerivedType(typeof(QuoteMarkup), "quote")]
public abstract class MarkupElement
{
    /// <summary>
    /// Начальный индекс элемента разметки (с нуля) в тексте.
    /// </summary>
    [JsonPropertyName("from")]
    public int From { get; set; }

    /// <summary>
    /// Длина элемента разметки.
    /// </summary>
    [JsonPropertyName("length")]
    public int Length { get; set; }
}

/// <summary>
/// Элемент разметки: полужирный текст (**bold**).
/// </summary>
public sealed class StrongMarkup : MarkupElement
{
}

/// <summary>
/// Элемент разметки: курсив (*italic*).
/// </summary>
public sealed class EmphasizedMarkup : MarkupElement
{
}

/// <summary>
/// Элемент разметки: моноширинный текст (`monospaced`) или блок кода (```code```).
/// </summary>
public sealed class MonospacedMarkup : MarkupElement
{
}

/// <summary>
/// Элемент разметки: ссылка в тексте.
/// </summary>
public sealed class LinkMarkup : MarkupElement
{
    /// <summary>
    /// URL ссылки.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Элемент разметки: зачеркнутый текст (~strikethrough~).
/// </summary>
public sealed class StrikethroughMarkup : MarkupElement
{
}

/// <summary>
/// Элемент разметки: подчеркнутый текст (++underline++).
/// </summary>
public sealed class UnderlineMarkup : MarkupElement
{
}

/// <summary>
/// Элемент разметки: заголовок в тексте.
/// </summary>
public sealed class HeadingMarkup : MarkupElement
{
}

/// <summary>
/// Элемент разметки: упоминание пользователя в тексте (по юзернейму или идентификатору).
/// </summary>
public sealed class UserMentionMarkup : MarkupElement
{
    /// <summary>
    /// Идентификатор упомянутого пользователя без юзернейма.
    /// </summary>
    [JsonPropertyName("user_id")]
    public long? UserId { get; set; }

    /// <summary>
    /// @username упомянутого пользователя.
    /// </summary>
    [JsonPropertyName("user_link")]
    public string? UserLink { get; set; }
}

/// <summary>
/// Элемент разметки: выделенный текст (highlighted).
/// </summary>
public sealed class HighlightedMarkup : MarkupElement
{
}

/// <summary>
/// Элемент разметки: блок цитаты в тексте.
/// </summary>
public sealed class QuoteMarkup : MarkupElement
{
}
