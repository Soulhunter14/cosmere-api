using Cross.Security;
using Messages.Matches.In;
using Messages.Matches.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Matches;

namespace API.Controllers;

[ApiController]
[Route("campaigns/{campaignId:long}/[controller]")]
[Authorize]
public class MatchesController(IMatchService matchService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MatchResponse>>> GetMatches(long campaignId)
        => Ok(await matchService.GetMatchesAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpGet("{matchId:long}")]
    public async Task<ActionResult<MatchResponse>> GetMatch(long campaignId, long matchId)
        => Ok(await matchService.GetMatchAsync(matchId, campaignId, JwtHelper.GetUserId(User)));

    [HttpPost]
    public async Task<ActionResult<MatchResponse>> CreateMatch(long campaignId, [FromBody] CreateMatchRequest request)
        => Ok(await matchService.CreateMatchAsync(campaignId, request, JwtHelper.GetUserId(User)));

    [HttpPost("{matchId:long}/scenes")]
    public async Task<ActionResult<MatchResponse>> AddScene(long campaignId, long matchId, [FromBody] AddSceneRequest request)
        => Ok(await matchService.AddSceneAsync(matchId, campaignId, request, JwtHelper.GetUserId(User)));

    [HttpPost("{matchId:long}/finalize")]
    public async Task<ActionResult<MatchResponse>> FinalizeMatch(long campaignId, long matchId, [FromBody] FinalizeMatchRequest request)
        => Ok(await matchService.FinalizeMatchAsync(matchId, campaignId, request, JwtHelper.GetUserId(User)));

    [HttpDelete("{matchId:long}")]
    public async Task<IActionResult> DeleteMatch(long campaignId, long matchId)
    {
        await matchService.DeleteMatchAsync(matchId, campaignId, JwtHelper.GetUserId(User));
        return NoContent();
    }
}
