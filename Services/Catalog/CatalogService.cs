using Infrastructure.Data;
using Messages.Catalog.In;
using Messages.Catalog.Out;
using Messages.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Services.Catalog;

public class CatalogService(CosmereContext db) : ICatalogService
{
    public async Task<List<WeaponCatalogResponse>> GetWeaponsAsync() =>
        await db.WeaponCatalog.Select(w => new WeaponCatalogResponse
        {
            Id = w.Id, Name = w.Name, WeaponTypeId = w.WeaponTypeId, SkillId = w.SkillId,
            DamageDiceCount = w.DamageDiceCount, DamageDiceValue = w.DamageDiceValue,
            DamageTypeId = w.DamageTypeId, RangeId = w.RangeId,
            TraitIds = w.TraitIds, ExpertTraitIds = w.ExpertTraitIds, IsCustom = w.IsCustom
        }).ToListAsync();

    public async Task<List<ArmorCatalogResponse>> GetArmorAsync() =>
        await db.ArmorCatalog.Select(a => new ArmorCatalogResponse
        {
            Id = a.Id, Name = a.Name, ArmorTypeId = a.ArmorTypeId, Desvio = a.Desvio,
            TraitIds = a.TraitIds, ExpertTraitIds = a.ExpertTraitIds, IsCustom = a.IsCustom
        }).ToListAsync();

    public async Task<List<GearItemResponse>> GetGearAsync() =>
        await db.GearItems.Select(g => new GearItemResponse
        {
            Id = g.Id, Name = g.Name, Weight = g.Weight, Price = g.Price, Description = g.Description
        }).ToListAsync();

    public async Task<List<CatalogOptionResponse>> GetOptionsByCategory(string category) =>
        await db.CatalogOptions
            .Where(o => o.Category.ToLower() == category.ToLower())
            .Select(o => new CatalogOptionResponse { Id = o.Id, Name = o.Name, Description = o.Description })
            .ToListAsync();

    public async Task<WeaponCatalogResponse> CreateWeaponAsync(CreateWeaponRequest request)
    {
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
        };
    }

    public async Task<ArmorCatalogResponse> CreateArmorAsync(CreateArmorRequest request)
    {
        var entity = new ArmorCatalogEntity
        {
            Name = request.Name,
            ArmorTypeId = request.ArmorTypeId,
            Desvio = request.Desvio,
            TraitIds = request.TraitIds,
            ExpertTraitIds = request.ExpertTraitIds,
            IsCustom = true,
        };
        db.ArmorCatalog.Add(entity);
        await db.SaveChangesAsync();
        return new ArmorCatalogResponse
        {
            Id = entity.Id, Name = entity.Name,
            ArmorTypeId = entity.ArmorTypeId, Desvio = entity.Desvio,
            TraitIds = entity.TraitIds, ExpertTraitIds = entity.ExpertTraitIds, IsCustom = entity.IsCustom,
        };
    }

    public async Task<bool> DeleteWeaponAsync(long id)
    {
        var entity = await db.WeaponCatalog.FindAsync(id);
        if (entity is null) return false;
        if (!entity.IsCustom) throw new InvalidOperationException("Cannot delete a seeded catalog item.");
        db.WeaponCatalog.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteArmorAsync(long id)
    {
        var entity = await db.ArmorCatalog.FindAsync(id);
        if (entity is null) return false;
        if (!entity.IsCustom) throw new InvalidOperationException("Cannot delete a seeded catalog item.");
        db.ArmorCatalog.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }

}
