using Messages.Mesa.Out;

namespace Services.Mesa;

/// <summary>The images the GM shows to the players at the table. Members read them; only the GM writes.</summary>
public interface IMesaService
{
    /// <summary>The campaign's images, oldest first, without their bytes. Any member.</summary>
    Task<CampaignImageResponse[]> ListImagesAsync(long campaignId, long userId);
    /// <summary>One image of the campaign; 404 if it does not exist or belongs to another campaign. Any member.</summary>
    Task<(byte[] Data, string ContentType)> GetImageAsync(long campaignId, long imageId, long userId);
    /// <summary>Stores an image (WebP, PNG or JPEG, 3 MB at most, 50 per campaign). GM only.</summary>
    Task<CampaignImageResponse> SaveImageAsync(long campaignId, string name, byte[] data, long userId);
    /// <summary>GM only; 404 if it does not exist or belongs to another campaign.</summary>
    Task DeleteImageAsync(long campaignId, long imageId, long userId);
}
