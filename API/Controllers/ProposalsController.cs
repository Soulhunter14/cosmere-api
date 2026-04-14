using Cross.Security;
using Messages.Proposals.In;
using Messages.Proposals.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Proposals;

namespace API.Controllers;

[ApiController]
[Route("campaigns/{campaignId:long}/[controller]")]
[Authorize]
public class ProposalsController(IProposalService proposalService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProposalResponse>>> GetProposals(long campaignId)
        => Ok(await proposalService.GetProposalsAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpPost]
    public async Task<ActionResult<ProposalResponse>> CreateProposal(long campaignId, [FromBody] CreateProposalRequest request)
        => Ok(await proposalService.CreateProposalAsync(campaignId, request, JwtHelper.GetUserId(User)));

    [HttpPost("{proposalId:long}/promote")]
    public async Task<ActionResult<ProposalResponse>> PromoteProposal(long campaignId, long proposalId, [FromBody] PromoteProposalRequest request)
        => Ok(await proposalService.PromoteProposalAsync(campaignId, proposalId, request, JwtHelper.GetUserId(User)));

    [HttpPost("{proposalId:long}/reject")]
    public async Task<ActionResult<ProposalResponse>> RejectProposal(long campaignId, long proposalId)
        => Ok(await proposalService.RejectProposalAsync(campaignId, proposalId, JwtHelper.GetUserId(User)));

    [HttpPut("{proposalId:long}/dates/{dateId:long}/vote")]
    public async Task<ActionResult<ProposalResponse>> CastVote(long campaignId, long proposalId, long dateId, [FromBody] CastVoteRequest request)
        => Ok(await proposalService.CastVoteAsync(campaignId, proposalId, dateId, request, JwtHelper.GetUserId(User)));
}
