using System.Text.Json.Serialization;

namespace MAX.Bot.Interfaces.Models.Request.Message.Link;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageLinkType
{
    Forward,
    Reply
}