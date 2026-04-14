using Messages.Proposals.In;
using Messages.Proposals.Out;

namespace Services.Proposals;

public interface IProposalService
{
    Task<List<ProposalResponse>> GetProposalsAsync(long campaignId, long userId);
    Task<ProposalResponse> CreateProposalAsync(long campaignId, CreateProposalRequest request, long userId);
    Task<ProposalResponse> PromoteProposalAsync(long campaignId, long proposalId, PromoteProposalRequest request, long userId);
    Task<ProposalResponse> RejectProposalAsync(long campaignId, long proposalId, long userId);
    Task<ProposalResponse> CastVoteAsync(long campaignId, long proposalId, long dateId, CastVoteRequest request, long userId);
}
