using MAX.Bot.Interfaces.Models.Request.Message.Attachment.Payloads;

namespace MAX.Bot.Interfaces.Models.Request.Message.Attachment;

/// <summary>
/// Вложение "Поделиться"
/// </summary>
public class ShareAttachment : AttachmentWithPayload
{
    public new SharePayload Payload { get; set; } = new();
}