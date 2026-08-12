namespace MAX.Bot.Interfaces.Models.Request.Message.Attachment;

public sealed class LocationAttachment : Attachment
{
    public LocationAttachment() { }

    public LocationAttachment(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}