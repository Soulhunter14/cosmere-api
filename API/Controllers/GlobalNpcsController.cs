using Cross.Security;
using Messages.GlobalNpcs.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.GlobalNpcs;

namespace API.Controllers;

/// <summary>
/// The adversaries of each world's catalog, read-only: they are the books', so an erratum or a new adversary comes in a migration
/// (like SeedMistbornAdversarios), and an enemy is adjusted on its own copy, in an encounter of the GM screen. Only their book
/// images are written.
/// </summary>
[ApiController]
[Route("global-npcs")]
[Authorize]
public class GlobalNpcsController(IGlobalNpcService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GlobalNpcResponse>>> GetAll([FromQuery] long? campaignId)
        => Ok(await service.GetAllAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<GlobalNpcResponse>> GetById(long id)
        => Ok(await service.GetByIdAsync(id));

    /// <summary>
    /// The adversary as its book prints it: «ficha» (stat block), «tacticas» (tactics box) or «retrato» (illustration). Through a
    /// campaign of the adversary's world, given by <paramref name="campaignId"/>: its GM reads the three, any member of it reads
    /// the «retrato».
    /// </summary>
    [HttpGet("{id:long}/book-images/{kind}")]
    public async Task<IActionResult> GetBookImage(long id, string kind, [FromQuery] long? campaignId)
    {
        var (data, contentType) = await service.GetBookImageAsync(id, kind, campaignId, JwtHelper.GetUserId(User));
        return File(data, contentType);
    }

    /// <summary>Uploads or replaces a book image (multipart, field «file»: WebP, PNG or JPEG).</summary>
    [HttpPut("{id:long}/book-images/{kind}")]
    [RequestSizeLimit(GlobalNpcService.MaxImageBytes + 64 * 1024)]
    public async Task<ActionResult<GlobalNpcResponse>> SaveBookImage(long id, string kind, IFormFile file, [FromQuery] long? campaignId)
    {
        if (file.Length > GlobalNpcService.MaxImageBytes) throw new ArgumentException("The image is too large.");
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return Ok(await service.SaveBookImageAsync(id, kind, ms.ToArray(), campaignId, JwtHelper.GetUserId(User)));
    }

    [HttpDelete("{id:long}/book-images/{kind}")]
    public async Task<IActionResult> DeleteBookImage(long id, string kind, [FromQuery] long? campaignId)
    {
        await service.DeleteBookImageAsync(id, kind, campaignId, JwtHelper.GetUserId(User));
        return NoContent();
    }
}
