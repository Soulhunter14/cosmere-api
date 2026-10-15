using API.Hubs;
using Cross.Security;
using Messages.Crew.In;
using Messages.Crew.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services.Crew;

namespace API.Controllers;

[ApiController]
[Route("campaigns/{campaignId:long}/crew")]
[Authorize]
public class CrewController(ICrewSheetService crewSheetService, IHubContext<CampaignHub> hubContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CrewSheetResponse>> GetSheet(long campaignId)
        => Ok(await crewSheetService.GetAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpPut]
    public async Task<ActionResult<CrewSheetResponse>> SaveSheet(long campaignId, [FromBody] SaveCrewSheetRequest request)
    {
        var result = await crewSheetService.SaveAsync(campaignId, request, JwtHelper.GetUserId(User));
        // 409 carries the sheet stored on the server, so the client can apply its change again on top of it
        if (!result.Saved)
            return Conflict(result.Sheet);

        await NotifyAsync(campaignId, "CrewSheetUpdated", result.Sheet.Version);
        return Ok(result.Sheet);
    }

    [HttpPost("aportes")]
    public async Task<ActionResult<CrewSheetResponse>> Aportar(long campaignId, [FromBody] AportarCrewRequest request)
    {
        var sheet = await crewSheetService.AportarAsync(campaignId, request, JwtHelper.GetUserId(User));
        await NotifyAsync(campaignId, "CrewSheetUpdated", sheet.Version);
        return Ok(sheet);
    }

    /// <summary>Goods leave a character's inventory and enter the crew's store, together</summary>
    [HttpPost("almacen/entradas")]
    public async Task<ActionResult<CrewSheetResponse>> EntrarAlmacen(long campaignId, [FromBody] EntrarAlmacenRequest request)
    {
        var sheet = await crewSheetService.EntrarAlmacenAsync(campaignId, request, JwtHelper.GetUserId(User));
        await NotifyAsync(campaignId, "CrewSheetUpdated", sheet.Version);
        return Ok(sheet);
    }

    /// <summary>A lot leaves the crew's store and its units enter a character's inventory, together</summary>
    [HttpPost("almacen/{loteId}/salida")]
    public async Task<ActionResult<CrewSheetResponse>> SacarAlmacen(long campaignId, string loteId, [FromBody] SacarAlmacenRequest request)
    {
        var sheet = await crewSheetService.SacarAlmacenAsync(campaignId, loteId, request, JwtHelper.GetUserId(User));
        await NotifyAsync(campaignId, "CrewSheetUpdated", sheet.Version);
        return Ok(sheet);
    }

    [HttpPut("sharing")]
    public async Task<ActionResult<CrewSheetResponse>> ShareSheet(long campaignId, [FromBody] ShareCrewSheetRequest request)
    {
        var sheet = await crewSheetService.ShareAsync(campaignId, request, JwtHelper.GetUserId(User));
        await NotifyAsync(campaignId, "CrewSheetShared", sheet.Version);
        return Ok(sheet);
    }

    // Only the campaign id and the version travel: the hub's JoinCampaign does not check membership, so no content of the sheet
    // may go out through it. Clients refetch what they are allowed to read.
    private Task NotifyAsync(long campaignId, string eventName, int version) =>
        hubContext.Clients.Group(CampaignHub.CampaignGroup(campaignId)).SendAsync(eventName, new { campaignId, version });
}
