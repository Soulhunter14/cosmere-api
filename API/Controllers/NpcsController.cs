using Cross.Security;
using Messages.Npcs.In;
using Messages.Npcs.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Npcs;

namespace API.Controllers;

[ApiController]
[Route("campaigns/{campaignId:long}/[controller]")]
[Authorize]
public class NpcsController(INpcService npcService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<NpcResponse>>> GetNpcs(long campaignId)
        => Ok(await npcService.GetNpcsAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpGet("{npcId:long}")]
    public async Task<ActionResult<NpcResponse>> GetNpc(long campaignId, long npcId)
        => Ok(await npcService.GetNpcAsync(npcId, campaignId, JwtHelper.GetUserId(User)));

    [HttpPost]
    public async Task<ActionResult<NpcResponse>> CreateNpc(long campaignId, [FromBody] CreateNpcRequest request)
        => Ok(await npcService.CreateNpcAsync(campaignId, request, JwtHelper.GetUserId(User)));

    [HttpPut("{npcId:long}")]
    public async Task<ActionResult<NpcResponse>> UpdateNpc(long campaignId, long npcId, [FromBody] UpdateNpcRequest request)
        => Ok(await npcService.UpdateNpcAsync(npcId, campaignId, request, JwtHelper.GetUserId(User)));

    [HttpPost("{npcId:long}/clone")]
    public async Task<ActionResult<NpcResponse>> CloneNpc(long campaignId, long npcId)
        => Ok(await npcService.CloneNpcAsync(npcId, campaignId, JwtHelper.GetUserId(User)));

    [HttpPatch("{npcId:long}/visibility")]
    public async Task<IActionResult> ToggleVisibility(long campaignId, long npcId, [FromBody] ToggleNpcVisibilityRequest request)
    {
        await npcService.ToggleVisibilityAsync(npcId, campaignId, request, JwtHelper.GetUserId(User));
        return NoContent();
    }

    [HttpDelete("{npcId:long}")]
    public async Task<IActionResult> DeleteNpc(long campaignId, long npcId)
    {
        await npcService.DeleteNpcAsync(npcId, campaignId, JwtHelper.GetUserId(User));
        return NoContent();
    }
}
