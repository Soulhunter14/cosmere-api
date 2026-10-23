using API.Hubs;
using Cross.Security;
using Messages.GmScreen.In;
using Messages.GmScreen.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services.GmScreen;

namespace API.Controllers;

[ApiController]
[Route("campaigns/{campaignId:long}/gm-screen")]
[Authorize]
public class GmScreenController(IGmScreenService gmScreenService, IHubContext<CampaignHub> hubContext, ILogger<GmScreenController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GmScreenResponse>> GetScreen(long campaignId)
        => Ok(await gmScreenService.GetAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpPut]
    public async Task<ActionResult<GmScreenResponse>> SaveScreen(long campaignId, [FromBody] SaveGmScreenRequest request)
    {
        var result = await gmScreenService.SaveAsync(campaignId, request, JwtHelper.GetUserId(User));
        // 409 carries the document stored on the server, so the client can reconcile with it
        if (!result.Saved)
            return Conflict(result.Screen);

        if (result.MesaChanged)
        {
            // Wake the players' tables. The event carries only the campaign and the version: JoinCampaign checks no membership,
            // so anything else would reach whoever joined the group; each client reads the document through GET …/mesa.
            logger.LogInformation("MesaUpdated sent for campaign {CampaignId} (version {Version})", campaignId, result.Screen.Version);
            await hubContext.Clients.Group(CampaignHub.CampaignGroup(campaignId))
                .SendAsync("MesaUpdated", new { campaignId, version = result.Screen.Version });
        }
        return Ok(result.Screen);
    }
}
