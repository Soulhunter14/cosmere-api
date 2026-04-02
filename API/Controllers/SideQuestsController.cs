using Cross.Security;
using Messages.SideQuests.In;
using Messages.SideQuests.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.SideQuests;

namespace API.Controllers;

[ApiController]
[Route("campaigns/{campaignId:long}/[controller]")]
[Authorize]
public class SideQuestsController(ISideQuestService sideQuestService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SideQuestResponse>>> GetSideQuests(long campaignId)
        => Ok(await sideQuestService.GetSideQuestsAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpGet("{sideQuestId:long}")]
    public async Task<ActionResult<SideQuestResponse>> GetSideQuest(long campaignId, long sideQuestId)
        => Ok(await sideQuestService.GetSideQuestAsync(sideQuestId, campaignId, JwtHelper.GetUserId(User)));

    [HttpPost]
    public async Task<ActionResult<SideQuestResponse>> CreateSideQuest(long campaignId, [FromBody] CreateSideQuestRequest request)
        => Ok(await sideQuestService.CreateSideQuestAsync(campaignId, request, JwtHelper.GetUserId(User)));

    [HttpPut("{sideQuestId:long}")]
    public async Task<ActionResult<SideQuestResponse>> UpdateSideQuest(long campaignId, long sideQuestId, [FromBody] UpdateSideQuestRequest request)
        => Ok(await sideQuestService.UpdateSideQuestAsync(sideQuestId, campaignId, request, JwtHelper.GetUserId(User)));

    [HttpDelete("{sideQuestId:long}")]
    public async Task<IActionResult> DeleteSideQuest(long campaignId, long sideQuestId)
    {
        await sideQuestService.DeleteSideQuestAsync(sideQuestId, campaignId, JwtHelper.GetUserId(User));
        return NoContent();
    }
}
