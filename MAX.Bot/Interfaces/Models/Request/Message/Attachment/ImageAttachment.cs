namespace MAX.Bot.Interfaces.Models.Request.Message.Attachment;

public sealed class ImageAttachment : AttachmentWithPayload
{
    public new Payloads.ImagePayload Payload { get; set; } = new();

    public override bool RequiresUploadProcessing => true;
}