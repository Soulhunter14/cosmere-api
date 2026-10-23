namespace Messages.Database.Entities;

/// <summary>
/// An image the director uploaded to show to the players at the table (a map, a portrait...) (AddCampaignImages), stored like the
/// adversaries' book images. Belongs to one campaign and is deleted with it. The server never reads inside it.
/// </summary>
public class CampaignImageEntity
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    /// <summary>What the director calls it (the file name by default), at most 120 characters.</summary>
    public required string Name { get; set; }
    /// <summary>image/webp, image/png or image/jpeg.</summary>
    public required string ContentType { get; set; }
    public byte[] Data { get; set; } = [];
    /// <summary>Size of <see cref="Data"/>, kept apart so listing never loads the image.</summary>
    public int Bytes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CampaignEntity Campaign { get; set; } = null!;
}
