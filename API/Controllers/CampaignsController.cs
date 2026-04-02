using Cross.Security;
using Messages.Campaigns.In;
using Messages.Campaigns.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Campaigns;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class CampaignsController(ICampaignService campaignService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CampaignResponse>>> GetCampaigns()
        => Ok(await campaignService.GetUserCampaignsAsync(JwtHelper.GetUserId(User)));

    [HttpGet("{campaignId:long}")]
    public async Task<ActionResult<CampaignDetailResponse>> GetCampaign(long campaignId)
        => Ok(await campaignService.GetCampaignDetailAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpPost]
    public async Task<ActionResult<CampaignDetailResponse>> CreateCampaign([FromBody] CreateCampaignRequest request)
        => Ok(await campaignService.CreateCampaignAsync(request, JwtHelper.GetUserId(User)));

    [HttpDelete("{campaignId:long}")]
    public async Task<IActionResult> DeleteCampaign(long campaignId)
    {
        await campaignService.DeleteCampaignAsync(campaignId, JwtHelper.GetUserId(User));
        return NoContent();
    }

    [HttpPost("join")]
    public async Task<ActionResult<CampaignResponse>> JoinCampaign([FromBody] JoinCampaignRequest request)
        => Ok(await campaignService.JoinCampaignAsync(request, JwtHelper.GetUserId(User)));

    [HttpPatch("{campaignId:long}/invite")]
    public async Task<IActionResult> UpdateInvite(long campaignId, [FromBody] UpdateInviteRequest request)
    {
        await campaignService.UpdateInviteAsync(campaignId, request, JwtHelper.GetUserId(User));
        return NoContent();
    }

    [HttpPost("{campaignId:long}/invite/regenerate")]
    public async Task<ActionResult<string>> RegenerateInviteCode(long campaignId)
        => Ok(await campaignService.RegenerateInviteCodeAsync(campaignId, JwtHelper.GetUserId(User)));
}
