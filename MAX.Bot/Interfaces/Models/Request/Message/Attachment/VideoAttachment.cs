namespace MAX.Bot.Interfaces.Models.Request.Message.Attachment;

public sealed class VideoAttachment : AttachmentWithPayload
{
    public new Payloads.VideoPayload Payload { get; set; } = new();

    public override bool RequiresUploadProcessing => true;
}