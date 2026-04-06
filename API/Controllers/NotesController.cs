using Cross.Security;
using Messages.Notes.In;
using Messages.Notes.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Notes;

namespace API.Controllers;

[ApiController]
[Route("campaigns/{campaignId:long}/[controller]")]
[Authorize]
public class NotesController(INoteService noteService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<NoteResponse>>> GetNotes(long campaignId)
        => Ok(await noteService.GetNotesAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpPost]
    public async Task<ActionResult<NoteResponse>> CreateNote(long campaignId, [FromBody] CreateNoteRequest request)
        => Ok(await noteService.CreateNoteAsync(campaignId, request, JwtHelper.GetUserId(User)));

    [HttpPut("{noteId:long}/read")]
    public async Task<IActionResult> MarkAsRead(long campaignId, long noteId)
    {
        await noteService.MarkAsReadAsync(noteId, campaignId, JwtHelper.GetUserId(User));
        return NoContent();
    }
}
