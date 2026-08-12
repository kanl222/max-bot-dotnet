using MAX.Bot.Interfaces.Models.Request.Message.Attachment.Payloads;

namespace MAX.Bot.Interfaces.Models.Request.Message.Attachment;

// Полиморфизм должен быть объявлен на базовом классе Attachment,
// т.к. именно этот тип используется в List<Attachment> при сериализации.
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(InlineKeyboardAttachment), "inline_keyboard")]
[JsonDerivedType(typeof(VideoAttachment), "video")]
[JsonDerivedType(typeof(ImageAttachment), "image")]
[JsonDerivedType(typeof(AudioAttachment), "audio")]
[JsonDerivedType(typeof(FileAttachment), "file")]
[JsonDerivedType(typeof(StickerAttachment), "sticker")]
[JsonDerivedType(typeof(ContactAttachment), "contact")]
[JsonDerivedType(typeof(ShareAttachment), "share")]
[JsonDerivedType(typeof(LocationAttachment), "location")]
public abstract class Attachment
{
    /// <summary>
    /// Возвращает предпочтительное имя файла для скачивания вложения.
    /// </summary>
    public virtual string? GetFileName() =>
        this is FileAttachment file ? file.Filename : null;

    /// <summary>
    /// Возвращает true, если вложение требует обработки (загрузки) на сервере перед отправкой сообщения.
    /// </summary>
    [JsonIgnore]
    public virtual bool RequiresUploadProcessing => false;
}

/// <summary>
/// Базовый класс для вложений, у которых данные передаются во вложенном "payload"
/// (image, video, file, audio, sticker, contact, share, inline_keyboard).
/// </summary>
public abstract class AttachmentWithPayload : Attachment
{
    [JsonPropertyName("payload")]
    public AttachmentPayload? Payload { get; set; }
}


public abstract class AttachmentPayload { }
