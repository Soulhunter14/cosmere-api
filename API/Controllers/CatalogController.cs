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
    [HttpGet("weapons")]
    public async Task<ActionResult<List<WeaponCatalogResponse>>> GetWeapons()
        => Ok(await catalogService.GetWeaponsAsync());

    [HttpGet("armor")]
    public async Task<ActionResult<List<ArmorCatalogResponse>>> GetArmor()
        => Ok(await catalogService.GetArmorAsync());

    [HttpGet("gear")]
    public async Task<ActionResult<List<GearItemResponse>>> GetGear()
        => Ok(await catalogService.GetGearAsync());

    [HttpGet("options/{category}")]
    public async Task<ActionResult<List<CatalogOptionResponse>>> GetOptions(string category)
        => Ok(await catalogService.GetOptionsByCategory(category));

    [HttpPost("weapons")]
    public async Task<ActionResult<WeaponCatalogResponse>> CreateWeapon([FromBody] CreateWeaponRequest request)
        => Ok(await catalogService.CreateWeaponAsync(request));

    [HttpPost("armor")]
    public async Task<ActionResult<ArmorCatalogResponse>> CreateArmor([FromBody] CreateArmorRequest request)
        => Ok(await catalogService.CreateArmorAsync(request));

    [HttpDelete("weapons/{id:long}")]
    public async Task<IActionResult> DeleteWeapon(long id)
    {
        try
        {
            var found = await catalogService.DeleteWeaponAsync(id);
            return found ? NoContent() : NotFound();
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }
    }

    [HttpDelete("armor/{id:long}")]
    public async Task<IActionResult> DeleteArmor(long id)
    {
        try
        {
            var found = await catalogService.DeleteArmorAsync(id);
            return found ? NoContent() : NotFound();
        }
        catch (InvalidOperationException)
        {
            return Forbid();
        }
    }

}
