namespace MAX.Bot.Interfaces.Models.Request.Message.Attachment;

public sealed class InlineKeyboardAttachment : AttachmentWithPayload
{
    public new Payloads.InlineKeyboardPayload Payload { get; set; } = new();

}