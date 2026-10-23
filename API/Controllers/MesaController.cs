using Cross.Security;
using Messages.GmScreen.Out;
using Messages.Mesa.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.GmScreen;
using Services.Mesa;

namespace API.Controllers;

/// <summary>
/// «La mesa»: what the GM shows to the players of a campaign, the public document derived from the GM screen and the images the
/// GM uploaded. Any member reads it; only the GM writes (the document through the GM screen, the images here).
/// </summary>
[ApiController]
[Authorize]
[Route("campaigns/{campaignId:long}/mesa")]
public class MesaController(IGmScreenService gmScreenService, IMesaService mesaService) : ControllerBase
{
    /// <summary>The public document of the screen; `{}` and version 0 when the GM has projected nothing yet.</summary>
    [HttpGet]
    public async Task<ActionResult<MesaResponse>> GetMesa(long campaignId)
        => Ok(await gmScreenService.GetMesaAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpGet("imagenes")]
    public async Task<ActionResult<CampaignImageResponse[]>> ListImages(long campaignId)
        => Ok(await mesaService.ListImagesAsync(campaignId, JwtHelper.GetUserId(User)));

    /// <summary>The bytes of one image. An image never changes (a new upload is a new image), so the browser may keep it.</summary>
    [HttpGet("imagenes/{imageId:long}")]
    public async Task<IActionResult> GetImage(long campaignId, long imageId)
    {
        var (data, contentType) = await mesaService.GetImageAsync(campaignId, imageId, JwtHelper.GetUserId(User));
        Response.Headers.CacheControl = "private, max-age=86400";
        return File(data, contentType);
    }

    /// <summary>Uploads an image (multipart, field «file»: WebP, PNG or JPEG). GM only.</summary>
    [HttpPost("imagenes")]
    [RequestSizeLimit(MesaService.MaxImageBytes + 64 * 1024)]
    public async Task<ActionResult<CampaignImageResponse>> SaveImage(long campaignId, IFormFile file)
    {
        if (file.Length > MesaService.MaxImageBytes) throw new ArgumentException("The image is too large.");
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return Ok(await mesaService.SaveImageAsync(campaignId, file.FileName, ms.ToArray(), JwtHelper.GetUserId(User)));
    }

    [HttpDelete("imagenes/{imageId:long}")]
    public async Task<IActionResult> DeleteImage(long campaignId, long imageId)
    {
        await mesaService.DeleteImageAsync(campaignId, imageId, JwtHelper.GetUserId(User));
        return NoContent();
    }
}
