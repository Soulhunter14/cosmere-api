using Cross.Security;
using Messages.GmScreen.In;
using Messages.GmScreen.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.GmScreen;

namespace API.Controllers;

[ApiController]
[Route("campaigns/{campaignId:long}/gm-screen")]
[Authorize]
public class GmScreenController(IGmScreenService gmScreenService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GmScreenResponse>> GetScreen(long campaignId)
        => Ok(await gmScreenService.GetAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpPut]
    public async Task<ActionResult<GmScreenResponse>> SaveScreen(long campaignId, [FromBody] SaveGmScreenRequest request)
    {
        var result = await gmScreenService.SaveAsync(campaignId, request, JwtHelper.GetUserId(User));
        // 409 carries the document stored on the server, so the client can reconcile with it
        return result.Saved ? Ok(result.Screen) : Conflict(result.Screen);
    }
}
