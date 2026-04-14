using Messages.Catalog.In;
using Messages.Catalog.Out;

namespace Services.Catalog;

public interface ICatalogService
{
    Task<List<WeaponCatalogResponse>> GetWeaponsAsync();
    Task<List<ArmorCatalogResponse>> GetArmorAsync();
    Task<List<GearItemResponse>> GetGearAsync();
    Task<List<CatalogOptionResponse>> GetOptionsByCategory(string category);
    Task<WeaponCatalogResponse> CreateWeaponAsync(CreateWeaponRequest request);
    Task<ArmorCatalogResponse> CreateArmorAsync(CreateArmorRequest request);
    Task<bool> DeleteWeaponAsync(long id);
    Task<bool> DeleteArmorAsync(long id);
    Task<bool> UpdateWeaponDescriptionAsync(long id, string description);
    Task<bool> UpdateArmorDescriptionAsync(long id, string description);
    Task<bool> UpdateGearDescriptionAsync(long id, string description);
}
