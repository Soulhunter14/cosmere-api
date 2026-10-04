using Infrastructure.Data;
using Messages.Catalog.In;
using Messages.Catalog.Out;
using Messages.Database.Entities;
using Messages.Worlds;
using Microsoft.EntityFrameworkCore;

namespace Services.Catalog;

public class CatalogService(CosmereContext db) : ICatalogService
{
    /// <summary>World and era (catalog number, <see cref="EraIds.Numero"/>) of the campaign a request is scoped to.</summary>
    private sealed record CampaignScope(string World, short? Era);

    // Items: World = @world AND (Era IS NULL OR @era IS NULL OR Era = @eraNum). Without campaignId: Stormlight, no era filter.
    public async Task<List<WeaponCatalogResponse>> GetWeaponsAsync(long? campaignId, long userId)
    {
        var scope = await ResolveScopeAsync(campaignId, userId, requireGm: false);
        var world = scope?.World ?? WorldIds.Stormlight;
        var query = db.WeaponCatalog.Where(w => w.World == world);
        if (scope?.Era is { } era) query = query.Where(w => w.Era == null || w.Era == era);
        return await query.Select(w => new WeaponCatalogResponse
        {
            Id = w.Id, Name = w.Name, WeaponTypeId = w.WeaponTypeId, SkillId = w.SkillId,
            DamageDiceCount = w.DamageDiceCount, DamageDiceValue = w.DamageDiceValue,
            DamageTypeId = w.DamageTypeId, RangeId = w.RangeId,
            TraitIds = w.TraitIds, ExpertTraitIds = w.ExpertTraitIds, IsCustom = w.IsCustom,
            Description = w.Description, Weight = w.Weight,
            World = w.World, Era = w.Era, IsRewardOnly = w.IsRewardOnly, Price = w.Price
        }).ToListAsync();
    }

    public async Task<List<ArmorCatalogResponse>> GetArmorAsync(long? campaignId, long userId)
    {
        var scope = await ResolveScopeAsync(campaignId, userId, requireGm: false);
        var world = scope?.World ?? WorldIds.Stormlight;
        var query = db.ArmorCatalog.Where(a => a.World == world);
        if (scope?.Era is { } era) query = query.Where(a => a.Era == null || a.Era == era);
        return await query.Select(a => new ArmorCatalogResponse
        {
            Id = a.Id, Name = a.Name, ArmorTypeId = a.ArmorTypeId, Desvio = a.Desvio,
            TraitIds = a.TraitIds, ExpertTraitIds = a.ExpertTraitIds, IsCustom = a.IsCustom,
            Description = a.Description, Weight = a.Weight,
            World = a.World, Era = a.Era, IsRewardOnly = a.IsRewardOnly, Price = a.Price
        }).ToListAsync();
    }

    public async Task<List<GearItemResponse>> GetGearAsync(long? campaignId, long userId)
    {
        var scope = await ResolveScopeAsync(campaignId, userId, requireGm: false);
        var world = scope?.World ?? WorldIds.Stormlight;
        var query = db.GearItems.Where(g => g.World == world);
        if (scope?.Era is { } era) query = query.Where(g => g.Era == null || g.Era == era);
        return await query.Select(g => new GearItemResponse
        {
            Id = g.Id, Name = g.Name, Weight = g.Weight, Price = g.Price, Description = g.Description,
            World = g.World, Era = g.Era, IsRewardOnly = g.IsRewardOnly, Category = g.Category
        }).ToListAsync();
    }

    // Options: World IN ('cosmere', @world); what is shared belongs to the Cosmere universe (P8). They have no era.
    public async Task<List<CatalogOptionResponse>> GetOptionsByCategory(string category, long? campaignId, long userId)
    {
        var scope = await ResolveScopeAsync(campaignId, userId, requireGm: false);
        var world = scope?.World ?? WorldIds.Stormlight;
        return await db.CatalogOptions
            .Where(o => o.Category.ToLower() == category.ToLower() && (o.World == WorldIds.Cosmere || o.World == world))
            .OrderBy(o => o.Id) // M3 rewrites rows with UPDATE, which changes the physical order a query without ORDER BY returns
            .Select(o => new CatalogOptionResponse { Id = o.Id, Name = o.Name, Description = o.Description, World = o.World })
            .ToListAsync();
    }

    public async Task<WeaponCatalogResponse> CreateWeaponAsync(CreateWeaponRequest request, long userId)
    {
        var scope = await ResolveScopeAsync(request.CampaignId, userId, requireGm: true);
        var entity = new WeaponCatalogEntity
        {
            Name = request.Name,
            WeaponTypeId = request.WeaponTypeId,
            SkillId = request.SkillId,
            DamageDiceCount = request.DamageDiceCount,
            DamageDiceValue = request.DamageDiceValue,
            DamageTypeId = request.DamageTypeId,
            RangeId = request.RangeId,
            TraitIds = request.TraitIds,
            ExpertTraitIds = request.ExpertTraitIds,
            IsCustom = true,
            Description = request.Description,
            Weight = request.Weight,
            Price = request.Price,
            IsRewardOnly = request.IsRewardOnly,
            World = scope?.World ?? WorldIds.Stormlight,
        };
        db.WeaponCatalog.Add(entity);
        await db.SaveChangesAsync();
        return new WeaponCatalogResponse
        {
            Id = entity.Id, Name = entity.Name,
            WeaponTypeId = entity.WeaponTypeId, SkillId = entity.SkillId,
            DamageDiceCount = entity.DamageDiceCount, DamageDiceValue = entity.DamageDiceValue,
            DamageTypeId = entity.DamageTypeId, RangeId = entity.RangeId,
            TraitIds = entity.TraitIds, ExpertTraitIds = entity.ExpertTraitIds, IsCustom = entity.IsCustom,
            Description = entity.Description, Weight = entity.Weight,
            World = entity.World, Era = entity.Era, IsRewardOnly = entity.IsRewardOnly, Price = entity.Price,
        };
    }

    public async Task<ArmorCatalogResponse> CreateArmorAsync(CreateArmorRequest request, long userId)
    {
        var scope = await ResolveScopeAsync(request.CampaignId, userId, requireGm: true);
        var entity = new ArmorCatalogEntity
        {
            Name = request.Name,
            ArmorTypeId = request.ArmorTypeId,
            Desvio = request.Desvio,
            TraitIds = request.TraitIds,
            ExpertTraitIds = request.ExpertTraitIds,
            IsCustom = true,
            Description = request.Description,
            Weight = request.Weight,
            Price = request.Price,
            IsRewardOnly = request.IsRewardOnly,
            World = scope?.World ?? WorldIds.Stormlight,
        };
        db.ArmorCatalog.Add(entity);
        await db.SaveChangesAsync();
        return new ArmorCatalogResponse
        {
            Id = entity.Id, Name = entity.Name,
            ArmorTypeId = entity.ArmorTypeId, Desvio = entity.Desvio,
            TraitIds = entity.TraitIds, ExpertTraitIds = entity.ExpertTraitIds, IsCustom = entity.IsCustom,
            Description = entity.Description, Weight = entity.Weight,
            World = entity.World, Era = entity.Era, IsRewardOnly = entity.IsRewardOnly, Price = entity.Price,
        };
    }

    public async Task<bool> DeleteWeaponAsync(long id, long? campaignId, long userId)
    {
        var scope = await ResolveScopeAsync(campaignId, userId, requireGm: true);
        var entity = await db.WeaponCatalog.FindAsync(id);
        if (entity is null || !InWorld(entity.World, scope)) return false; // before IsCustom: a seeded item of another world is a 404
        if (!entity.IsCustom) throw new InvalidOperationException("Cannot delete a seeded catalog item.");
        db.WeaponCatalog.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteArmorAsync(long id, long? campaignId, long userId)
    {
        var scope = await ResolveScopeAsync(campaignId, userId, requireGm: true);
        var entity = await db.ArmorCatalog.FindAsync(id);
        if (entity is null || !InWorld(entity.World, scope)) return false; // before IsCustom: a seeded item of another world is a 404
        if (!entity.IsCustom) throw new InvalidOperationException("Cannot delete a seeded catalog item.");
        db.ArmorCatalog.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateWeaponDescriptionAsync(long id, string description, long? campaignId, long userId)
    {
        var scope = await ResolveScopeAsync(campaignId, userId, requireGm: true);
        var entity = await db.WeaponCatalog.FindAsync(id);
        if (entity is null || !InWorld(entity.World, scope)) return false;
        entity.Description = description;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateArmorDescriptionAsync(long id, string description, long? campaignId, long userId)
    {
        var scope = await ResolveScopeAsync(campaignId, userId, requireGm: true);
        var entity = await db.ArmorCatalog.FindAsync(id);
        if (entity is null || !InWorld(entity.World, scope)) return false;
        entity.Description = description;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateGearDescriptionAsync(long id, string description, long? campaignId, long userId)
    {
        var scope = await ResolveScopeAsync(campaignId, userId, requireGm: true);
        var entity = await db.GearItems.FindAsync(id);
        if (entity is null || !InWorld(entity.World, scope)) return false;
        entity.Description = description;
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>Without campaignId there is no world check (as before); with it the item must belong to the campaign's world.</summary>
    private static bool InWorld(string itemWorld, CampaignScope? scope) => scope is null || itemWorld == scope.World;

    /// <summary>
    /// Resolves the campaign of a request; <c>null</c> without campaignId (cached client: behaves as always, P1).
    /// With it: 404 if the campaign does not exist; 403 if the caller is not a member of it or, with
    /// <paramref name="requireGm"/>, not its GM. Same CampaignMembers queries as the other services (EnsureMemberAsync / IsGmAsync).
    /// </summary>
    private async Task<CampaignScope?> ResolveScopeAsync(long? campaignId, long userId, bool requireGm)
    {
        if (campaignId is null) return null;
        var cid = campaignId.Value;
        var campaign = await db.Campaigns.AsNoTracking()
            .Where(c => c.Id == cid)
            .Select(c => new { c.World, c.Era })
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Campaign not found.");

        if (requireGm)
        {
            var isGm = await db.CampaignMembers.AnyAsync(m => m.CampaignId == cid && m.UserId == userId && m.Role == "gm");
            if (!isGm) throw new UnauthorizedAccessException("Only the GM can perform this action.");
        }
        else
        {
            var isMember = await db.CampaignMembers.AnyAsync(m => m.CampaignId == cid && m.UserId == userId);
            if (!isMember) throw new UnauthorizedAccessException("You are not a member of this campaign.");
        }

        return new CampaignScope(campaign.World, EraIds.Numero(campaign.Era));
    }
}
