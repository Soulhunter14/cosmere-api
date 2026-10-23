using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.Mesa.Out;
using Microsoft.EntityFrameworkCore;
using Services.Images;

namespace Services.Mesa;

public class MesaService(CosmereContext db) : IMesaService
{
    /// <summary>Largest accepted image (a map or a portrait as WebP weighs well under it).</summary>
    public const int MaxImageBytes = 3 * 1024 * 1024;
    /// <summary>Most images a campaign can keep.</summary>
    public const int MaxImagesPerCampaign = 50;
    private const int MaxNameLength = 120;

    public async Task<CampaignImageResponse[]> ListImagesAsync(long campaignId, long userId)
    {
        await RequireRoleAsync(campaignId, userId, requireGm: false);

        // Project the columns: the bytes of the images are never loaded to list them
        return await db.CampaignImages.AsNoTracking()
            .Where(i => i.CampaignId == campaignId)
            .OrderBy(i => i.CreatedAt).ThenBy(i => i.Id)
            .Select(i => new CampaignImageResponse
            {
                Id = i.Id, Name = i.Name, ContentType = i.ContentType, Bytes = i.Bytes, CreatedAt = i.CreatedAt,
            })
            .ToArrayAsync();
    }

    public async Task<(byte[] Data, string ContentType)> GetImageAsync(long campaignId, long imageId, long userId)
    {
        await RequireRoleAsync(campaignId, userId, requireGm: false);

        var image = await db.CampaignImages.AsNoTracking()
            .Where(i => i.Id == imageId && i.CampaignId == campaignId)
            .Select(i => new { i.Data, i.ContentType })
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Image not found.");
        return (image.Data, image.ContentType);
    }

    public async Task<CampaignImageResponse> SaveImageAsync(long campaignId, string name, byte[] data, long userId)
    {
        await RequireRoleAsync(campaignId, userId, requireGm: true);

        if (data.Length == 0 || data.Length > MaxImageBytes)
            throw new ArgumentException($"The image must weigh between 1 byte and {MaxImageBytes / 1024 / 1024} MB.");
        var type = ImageType.Detect(data) ?? throw new ArgumentException("The image must be WebP, PNG or JPEG.");
        if (await db.CampaignImages.CountAsync(i => i.CampaignId == campaignId) >= MaxImagesPerCampaign)
            throw new ArgumentException($"The campaign already has {MaxImagesPerCampaign} images.");

        var trimmed = (name ?? "").Trim();
        if (trimmed.Length > MaxNameLength) trimmed = trimmed[..MaxNameLength].TrimEnd();
        var entity = new CampaignImageEntity
        {
            CampaignId = campaignId,
            Name = trimmed.Length == 0 ? "imagen" : trimmed,
            ContentType = type,
            Data = data,
            Bytes = data.Length,
        };
        db.CampaignImages.Add(entity);
        await db.SaveChangesAsync();

        return new CampaignImageResponse
        {
            Id = entity.Id, Name = entity.Name, ContentType = entity.ContentType, Bytes = entity.Bytes, CreatedAt = entity.CreatedAt,
        };
    }

    public async Task DeleteImageAsync(long campaignId, long imageId, long userId)
    {
        await RequireRoleAsync(campaignId, userId, requireGm: true);

        var image = await db.CampaignImages.FirstOrDefaultAsync(i => i.Id == imageId && i.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Image not found.");
        db.CampaignImages.Remove(image);
        await db.SaveChangesAsync();
    }

    /// <summary>Not a member → 404 (the campaign is not revealed); a member who is not the GM, when the GM is required → 403.</summary>
    private async Task RequireRoleAsync(long campaignId, long userId, bool requireGm)
    {
        var role = await db.CampaignMembers
            .Where(m => m.CampaignId == campaignId && m.UserId == userId)
            .Select(m => m.Role)
            .FirstOrDefaultAsync();

        if (role is null)
            throw new KeyNotFoundException("Campaign not found.");
        if (requireGm && role != "gm")
            throw new UnauthorizedAccessException("Only the GM can perform this action.");
    }
}
