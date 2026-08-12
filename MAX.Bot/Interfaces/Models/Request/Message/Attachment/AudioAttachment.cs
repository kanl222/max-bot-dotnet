namespace MAX.Bot.Interfaces.Models.Request.Message.Attachment;

public sealed class AudioAttachment : AttachmentWithPayload
{
    public new Payloads.AudioPayload Payload { get; set; } = new();

    public override bool RequiresUploadProcessing => true;
}