using Cross.Security;
using Messages.Catalog.In;
using Messages.Catalog.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Catalog;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class CatalogController(ICatalogService catalogService) : ControllerBase
{
    // campaignId scopes the catalog to the world (and era) of that campaign: members read, only its GM writes.
    // Without it everything behaves as it always did (Stormlight, any authenticated user), so cached clients keep working.
    [HttpGet("weapons")]
    public async Task<ActionResult<List<WeaponCatalogResponse>>> GetWeapons([FromQuery] long? campaignId)
        => Ok(await catalogService.GetWeaponsAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpGet("armor")]
    public async Task<ActionResult<List<ArmorCatalogResponse>>> GetArmor([FromQuery] long? campaignId)
        => Ok(await catalogService.GetArmorAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpGet("gear")]
    public async Task<ActionResult<List<GearItemResponse>>> GetGear([FromQuery] long? campaignId)
        => Ok(await catalogService.GetGearAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpGet("options/{category}")]
    public async Task<ActionResult<List<CatalogOptionResponse>>> GetOptions(string category, [FromQuery] long? campaignId)
        => Ok(await catalogService.GetOptionsByCategory(category, campaignId, JwtHelper.GetUserId(User)));

    // The campaign of the new item travels in the body (CampaignId).
    [HttpPost("weapons")]
    public async Task<ActionResult<WeaponCatalogResponse>> CreateWeapon([FromBody] CreateWeaponRequest request)
        => Ok(await catalogService.CreateWeaponAsync(request, JwtHelper.GetUserId(User)));

    [HttpPost("armor")]
    public async Task<ActionResult<ArmorCatalogResponse>> CreateArmor([FromBody] CreateArmorRequest request)
        => Ok(await catalogService.CreateArmorAsync(request, JwtHelper.GetUserId(User)));

    [HttpDelete("weapons/{id:long}")]
    public async Task<IActionResult> DeleteWeapon(long id, [FromQuery] long? campaignId)
    {
        try
        {
            var found = await catalogService.DeleteWeaponAsync(id, campaignId, JwtHelper.GetUserId(User));
            return found ? NoContent() : NotFound();
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }
    }

    [HttpDelete("armor/{id:long}")]
    public async Task<IActionResult> DeleteArmor(long id, [FromQuery] long? campaignId)
    {
        try
        {
            var found = await catalogService.DeleteArmorAsync(id, campaignId, JwtHelper.GetUserId(User));
            return found ? NoContent() : NotFound();
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }
    }

    [HttpPut("weapons/{id:long}/description")]
    public async Task<IActionResult> UpdateWeaponDescription(long id, [FromBody] UpdateDescriptionRequest request, [FromQuery] long? campaignId)
    {
        var found = await catalogService.UpdateWeaponDescriptionAsync(id, request.Description, campaignId, JwtHelper.GetUserId(User));
        return found ? NoContent() : NotFound();
    }

    [HttpPut("armor/{id:long}/description")]
    public async Task<IActionResult> UpdateArmorDescription(long id, [FromBody] UpdateDescriptionRequest request, [FromQuery] long? campaignId)
    {
        var found = await catalogService.UpdateArmorDescriptionAsync(id, request.Description, campaignId, JwtHelper.GetUserId(User));
        return found ? NoContent() : NotFound();
    }

    [HttpPut("gear/{id:long}/description")]
    public async Task<IActionResult> UpdateGearDescription(long id, [FromBody] UpdateDescriptionRequest request, [FromQuery] long? campaignId)
    {
        var found = await catalogService.UpdateGearDescriptionAsync(id, request.Description, campaignId, JwtHelper.GetUserId(User));
        return found ? NoContent() : NotFound();
    }

}
