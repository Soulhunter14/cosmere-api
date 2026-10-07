using Messages.Catalog.In;
using Messages.Catalog.Out;

namespace Services.Catalog;

/// <remarks>
/// <c>campaignId</c> scopes the catalog to the world (and era) of that campaign. Without it the service behaves as it
/// always did (Stormlight, no era filter, no role check), so a cached client keeps working (P1).
/// With it: 404 if the campaign does not exist and 403 if the caller is not a member (reads) or not its GM (writes).
/// </remarks>
public interface ICatalogService
{
    Task<List<WeaponCatalogResponse>> GetWeaponsAsync(long? campaignId, long userId);
    Task<List<ArmorCatalogResponse>> GetArmorAsync(long? campaignId, long userId);
    Task<List<GearItemResponse>> GetGearAsync(long? campaignId, long userId);
    Task<List<CatalogOptionResponse>> GetOptionsByCategory(string category, long? campaignId, long userId);

    /// <summary>The campaign travels in <see cref="CreateWeaponRequest.CampaignId"/>; the new item takes its world.</summary>
    Task<WeaponCatalogResponse> CreateWeaponAsync(CreateWeaponRequest request, long userId);

    /// <summary>The campaign travels in <see cref="CreateArmorRequest.CampaignId"/>; the new item takes its world.</summary>
    Task<ArmorCatalogResponse> CreateArmorAsync(CreateArmorRequest request, long userId);

    // With campaignId the item must belong to the world of the campaign; otherwise they return false (the controller answers 404).
    Task<bool> DeleteWeaponAsync(long id, long? campaignId, long userId);
    Task<bool> DeleteArmorAsync(long id, long? campaignId, long userId);
    Task<bool> UpdateWeaponDescriptionAsync(long id, string description, long? campaignId, long userId);
    Task<bool> UpdateArmorDescriptionAsync(long id, string description, long? campaignId, long userId);
    Task<bool> UpdateGearDescriptionAsync(long id, string description, long? campaignId, long userId);
}
