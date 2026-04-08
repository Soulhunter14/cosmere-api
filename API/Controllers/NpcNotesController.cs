using Cross.Security;
using Messages.NpcNotes.In;
using Messages.NpcNotes.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.NpcNotes;

namespace API.Controllers;

[ApiController]
[Route("campaigns/{campaignId:long}/npc-notes")]
[Authorize]
public class NpcNotesController(INpcNoteService npcNoteService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<NpcNoteResponse>>> GetNpcNotes(long campaignId)
        => Ok(await npcNoteService.GetNpcNotesAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpPost]
    public async Task<ActionResult<NpcNoteResponse>> CreateNpcNote(long campaignId, [FromBody] CreateNpcNoteRequest request)
        => Ok(await npcNoteService.CreateNpcNoteAsync(campaignId, request, JwtHelper.GetUserId(User)));

    [HttpPut("{noteId:long}")]
    public async Task<ActionResult<NpcNoteResponse>> UpdateNpcNote(long campaignId, long noteId, [FromBody] UpdateNpcNoteRequest request)
        => Ok(await npcNoteService.UpdateNpcNoteAsync(noteId, campaignId, request, JwtHelper.GetUserId(User)));

    [HttpDelete("{noteId:long}")]
    public async Task<IActionResult> DeleteNpcNote(long campaignId, long noteId)
    {
        await npcNoteService.DeleteNpcNoteAsync(noteId, campaignId, JwtHelper.GetUserId(User));
        return NoContent();
    }
}
