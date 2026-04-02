using Cross.Security;
using Messages.Sessions.In;
using Messages.Sessions.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Sessions;

namespace API.Controllers;

[ApiController]
[Route("campaigns/{campaignId:long}/[controller]")]
[Authorize]
public class SessionsController(ISessionService sessionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SessionResponse>>> GetSessions(long campaignId)
        => Ok(await sessionService.GetSessionsAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpPost]
    public async Task<ActionResult<SessionResponse>> CreateSession(long campaignId, [FromBody] CreateSessionRequest request)
        => Ok(await sessionService.CreateSessionAsync(campaignId, request, JwtHelper.GetUserId(User)));

    [HttpDelete("{sessionId:long}")]
    public async Task<IActionResult> DeleteSession(long campaignId, long sessionId)
    {
        await sessionService.DeleteSessionAsync(sessionId, campaignId, JwtHelper.GetUserId(User));
        return NoContent();
    }
}
