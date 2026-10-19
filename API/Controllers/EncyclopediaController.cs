using Cross.Security;
using Messages.Encyclopedia.In;
using Messages.Encyclopedia.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Encyclopedia;

namespace API.Controllers;

/// <summary>
/// The originals of the encyclopedia: the book's text (Markdown) and a mini-PDF of each entry (a heroic path, a metalborn path, a
/// power…), shared by every campaign of the world of <c>campaignId</c>. Any member reads them; only the GM uploads or removes them.
/// </summary>
[ApiController]
[Route("encyclopedia/originals")]
[Authorize]
public class EncyclopediaController(IEncyclopediaService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<EncyclopediaOriginalSummaryResponse>>> List([FromQuery] long campaignId)
        => Ok(await service.ListAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpGet("{section}/{key}/text")]
    public async Task<ActionResult<EncyclopediaTextResponse>> GetText(string section, string key, [FromQuery] long campaignId)
        => Ok(await service.GetTextAsync(campaignId, section, key, JwtHelper.GetUserId(User)));

    [HttpPut("{section}/{key}/text")]
    public async Task<ActionResult<EncyclopediaOriginalSummaryResponse>> SaveText(
        string section, string key, [FromQuery] long campaignId, [FromBody] SaveEncyclopediaTextRequest request)
        => Ok(await service.SaveTextAsync(campaignId, section, key, request, JwtHelper.GetUserId(User)));

    [HttpDelete("{section}/{key}/text")]
    public async Task<IActionResult> DeleteText(string section, string key, [FromQuery] long campaignId)
    {
        await service.DeleteTextAsync(campaignId, section, key, JwtHelper.GetUserId(User));
        return NoContent();
    }

    [HttpGet("{section}/{key}/pdf")]
    public async Task<IActionResult> GetPdf(string section, string key, [FromQuery] long campaignId)
    {
        var (data, _) = await service.GetPdfAsync(campaignId, section, key, JwtHelper.GetUserId(User));
        return File(data, "application/pdf", $"{key}.pdf");
    }

    /// <summary>Uploads or replaces the mini-PDF (multipart: field «file», the PDF, and «title», the title of the entry).</summary>
    [HttpPut("{section}/{key}/pdf")]
    [RequestSizeLimit(EncyclopediaService.MaxPdfBytes + 64 * 1024)]
    public async Task<ActionResult<EncyclopediaOriginalSummaryResponse>> SavePdf(
        string section, string key, [FromQuery] long campaignId, IFormFile file, [FromForm] string title)
    {
        if (file.Length > EncyclopediaService.MaxPdfBytes) throw new ArgumentException("The PDF is too large.");
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return Ok(await service.SavePdfAsync(campaignId, section, key, title, ms.ToArray(), JwtHelper.GetUserId(User)));
    }

    [HttpDelete("{section}/{key}/pdf")]
    public async Task<IActionResult> DeletePdf(string section, string key, [FromQuery] long campaignId)
    {
        await service.DeletePdfAsync(campaignId, section, key, JwtHelper.GetUserId(User));
        return NoContent();
    }
}
