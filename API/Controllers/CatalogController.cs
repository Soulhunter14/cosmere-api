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

    [HttpPost("reimport")]
    public async Task<IActionResult> Reimport()
    {
        await catalogService.ReimportAsync();
        return NoContent();
    }
}
