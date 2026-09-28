using API.Hubs;
using Cross.Security;
using Messages.DiceRolls.In;
using Messages.DiceRolls.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services.DiceRolls;

namespace API.Controllers;

[ApiController]
[Route("campaigns/{campaignId:long}/dice-rolls")]
[Authorize]
public class DiceRollsController(
    IDiceRollService diceRollService,
    IHubContext<CampaignHub> hubContext
) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DiceRollResponse>>> GetRecent(
        long campaignId,
        [FromQuery] int limit = 50)
    {
        var rolls = await diceRollService.GetRecentAsync(campaignId, JwtHelper.GetUserId(User), limit);
        return Ok(rolls);
    }

    [HttpPost]
    public async Task<ActionResult<DiceRollResponse>> Create(
        long campaignId,
        [FromBody] CreateDiceRollRequest request)
    {
        var roll = await diceRollService.CreateAsync(campaignId, request, JwtHelper.GetUserId(User));

        // Broadcast to everyone in the campaign group (including the sender)
        await hubContext.Clients
            .Group(CampaignHub.CampaignGroup(campaignId))
            .SendAsync("DiceRollReceived", roll);

        return Ok(roll);
    }
}
