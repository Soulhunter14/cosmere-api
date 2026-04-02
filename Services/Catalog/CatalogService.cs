using Infrastructure.Data;
using Messages.Catalog.Out;
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
            TraitIds = w.TraitIds, ExpertTraitIds = w.ExpertTraitIds
        }).ToListAsync();

    public async Task<List<ArmorCatalogResponse>> GetArmorAsync() =>
        await db.ArmorCatalog.Select(a => new ArmorCatalogResponse
        {
            Id = a.Id, Name = a.Name, ArmorTypeId = a.ArmorTypeId, Desvio = a.Desvio,
            TraitIds = a.TraitIds, ExpertTraitIds = a.ExpertTraitIds
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

    public async Task ReimportAsync()
    {
        db.CatalogOptions.RemoveRange(db.CatalogOptions);
        db.WeaponCatalog.RemoveRange(db.WeaponCatalog);
        db.ArmorCatalog.RemoveRange(db.ArmorCatalog);
        db.GearItems.RemoveRange(db.GearItems);
        await db.SaveChangesAsync();
        SeedData.SeedCatalog(db);
        await db.SaveChangesAsync();
    }
}
