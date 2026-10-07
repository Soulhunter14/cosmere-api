using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.Proposals;
using Messages.Proposals.In;
using Messages.Proposals.Out;
using Microsoft.EntityFrameworkCore;

namespace Services.Proposals;

public class ProposalService(CosmereContext db) : IProposalService
{
    public async Task<List<ProposalResponse>> GetProposalsAsync(long campaignId, long userId)
    {
        var isMember = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId);
        if (!isMember) throw new KeyNotFoundException("Campaign not found.");

        var proposals = await db.SessionProposals
            .Where(p => p.CampaignId == campaignId)
            .Include(p => p.ProposedDates)
                .ThenInclude(d => d.Votes)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return proposals.Select(p => MapToResponse(p, userId)).ToList();
    }

    public async Task<ProposalResponse> CreateProposalAsync(long campaignId, CreateProposalRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        // Slots win over plain dates; older clients only send plain dates (no slot).
        var slots = request.ProposedSlots
            ?? request.ProposedDates.Select(d => new ProposedSlotRequest { Date = d }).ToList();

        if (slots.Count == 0)
            throw new ArgumentException("At least one proposed date is required.");
        if (slots.Any(s => !ProposalSlots.IsValid(s.Slot)))
            throw new ArgumentException("Unknown proposal slot.");

        var proposal = new SessionProposalEntity
        {
            CampaignId = campaignId,
            Title = request.Title,
            Notes = request.Notes,
            ProposedDates = slots
                .Select(s => new ProposalDateEntity { ProposedDate = s.Date.ToUniversalTime(), Slot = s.Slot })
                .ToList()
        };

        db.SessionProposals.Add(proposal);
        await db.SaveChangesAsync();

        // Reload with votes (empty at creation)
        var created = await db.SessionProposals
            .Include(p => p.ProposedDates)
                .ThenInclude(d => d.Votes)
            .FirstAsync(p => p.Id == proposal.Id);

        return MapToResponse(created, userId);
    }

    public async Task<ProposalResponse> PromoteProposalAsync(long campaignId, long proposalId, PromoteProposalRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        var proposal = await db.SessionProposals
            .Include(p => p.ProposedDates)
                .ThenInclude(d => d.Votes)
            .FirstOrDefaultAsync(p => p.Id == proposalId && p.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Proposal not found.");

        if (proposal.Status != "Pending")
            throw new InvalidOperationException("Only pending proposals can be promoted.");

        var chosenDate = proposal.ProposedDates.FirstOrDefault(d => d.Id == request.ProposalDateId)
            ?? throw new KeyNotFoundException("Proposed date not found in this proposal.");

        var session = new SessionEntity
        {
            CampaignId = campaignId,
            Title = request.Title,
            Date = chosenDate.ProposedDate,
            Location = request.Location,
            Notes = proposal.Notes,
        };

        db.Sessions.Add(session);
        await db.SaveChangesAsync();

        chosenDate.Status = "Accepted";
        chosenDate.SessionId = session.Id;
        foreach (var d in proposal.ProposedDates.Where(d => d.Status == "Pending"))
            d.Status = "Rejected";

        proposal.Status = "Promoted";
        proposal.ResolvedAt = DateTime.UtcNow;
        proposal.PromotedSessionId = session.Id;

        await db.SaveChangesAsync();
        return MapToResponse(proposal, userId);
    }

    public async Task<ProposalResponse> PromoteDateAsync(long campaignId, long proposalId, long dateId, PromoteDateRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        var proposal = await LoadPendingProposalAsync(campaignId, proposalId);
        var date = PendingDate(proposal, dateId);

        var session = new SessionEntity
        {
            CampaignId = campaignId,
            Title = request.Title,
            Date = date.ProposedDate,
            Location = request.Location,
            Notes = proposal.Notes,
        };

        db.Sessions.Add(session);
        await db.SaveChangesAsync();

        date.Status = "Accepted";
        date.SessionId = session.Id;
        ResolveIfDone(proposal);

        await db.SaveChangesAsync();
        return MapToResponse(proposal, userId);
    }

    public async Task<ProposalResponse> RejectDateAsync(long campaignId, long proposalId, long dateId, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        var proposal = await LoadPendingProposalAsync(campaignId, proposalId);
        var date = PendingDate(proposal, dateId);

        date.Status = "Rejected";
        ResolveIfDone(proposal);

        await db.SaveChangesAsync();
        return MapToResponse(proposal, userId);
    }

    public async Task<ProposalResponse> RejectProposalAsync(long campaignId, long proposalId, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        var proposal = await db.SessionProposals
            .Include(p => p.ProposedDates)
                .ThenInclude(d => d.Votes)
            .FirstOrDefaultAsync(p => p.Id == proposalId && p.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Proposal not found.");

        if (proposal.Status != "Pending")
            throw new InvalidOperationException("Only pending proposals can be rejected.");

        // Closes the proposal: dates still pending are rejected; dates already promoted keep their session.
        foreach (var d in proposal.ProposedDates.Where(d => d.Status == "Pending"))
            d.Status = "Rejected";
        ResolveIfDone(proposal);

        await db.SaveChangesAsync();
        return MapToResponse(proposal, userId);
    }

    public async Task<ProposalResponse> CastVoteAsync(long campaignId, long proposalId, long dateId, CastVoteRequest request, long userId)
    {
        var isMember = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId);
        if (!isMember) throw new KeyNotFoundException("Campaign not found.");

        var proposal = await db.SessionProposals
            .Include(p => p.ProposedDates)
                .ThenInclude(d => d.Votes)
            .FirstOrDefaultAsync(p => p.Id == proposalId && p.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Proposal not found.");

        if (proposal.Status != "Pending")
            throw new InvalidOperationException("Cannot vote on a resolved proposal.");

        var proposalDate = proposal.ProposedDates.FirstOrDefault(d => d.Id == dateId)
            ?? throw new KeyNotFoundException("Proposed date not found.");

        if (proposalDate.Status != "Pending")
            throw new InvalidOperationException("Cannot vote on a resolved date.");

        var existingVote = proposalDate.Votes.FirstOrDefault(v => v.UserId == userId);
        if (existingVote is not null)
        {
            existingVote.CanAttend = request.CanAttend;
            existingVote.VotedAt = DateTime.UtcNow;
        }
        else
        {
            proposalDate.Votes.Add(new ProposalVoteEntity
            {
                ProposalDateId = dateId,
                UserId = userId,
                CanAttend = request.CanAttend,
            });
        }

        await db.SaveChangesAsync();
        return MapToResponse(proposal, userId);
    }

    private async Task<SessionProposalEntity> LoadPendingProposalAsync(long campaignId, long proposalId)
    {
        var proposal = await db.SessionProposals
            .Include(p => p.ProposedDates)
                .ThenInclude(d => d.Votes)
            .FirstOrDefaultAsync(p => p.Id == proposalId && p.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Proposal not found.");

        if (proposal.Status != "Pending")
            throw new InvalidOperationException("The proposal is already resolved.");

        return proposal;
    }

    private static ProposalDateEntity PendingDate(SessionProposalEntity proposal, long dateId)
    {
        var date = proposal.ProposedDates.FirstOrDefault(d => d.Id == dateId)
            ?? throw new KeyNotFoundException("Proposed date not found in this proposal.");

        if (date.Status != "Pending")
            throw new InvalidOperationException("This date is already resolved.");

        return date;
    }

    /// <summary>Closes the proposal once no date is pending: promoted if any date was accepted, rejected otherwise.</summary>
    private static void ResolveIfDone(SessionProposalEntity proposal)
    {
        if (proposal.ProposedDates.Any(d => d.Status == "Pending")) return;

        var accepted = proposal.ProposedDates
            .Where(d => d.Status == "Accepted")
            .OrderBy(d => d.ProposedDate)
            .ToList();

        proposal.Status = accepted.Count > 0 ? "Promoted" : "Rejected";
        proposal.ResolvedAt = DateTime.UtcNow;
        proposal.PromotedSessionId ??= accepted.FirstOrDefault()?.SessionId;
    }

    private async Task EnsureGmAsync(long campaignId, long userId)
    {
        var isGm = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId && m.Role == "gm");
        if (!isGm) throw new UnauthorizedAccessException("Only the GM can perform this action.");
    }

    private static ProposalResponse MapToResponse(SessionProposalEntity p, long currentUserId) => new()
    {
        Id = p.Id,
        CampaignId = p.CampaignId,
        Title = p.Title,
        Notes = p.Notes,
        Status = p.Status,
        CreatedAt = p.CreatedAt,
        ResolvedAt = p.ResolvedAt,
        PromotedSessionId = p.PromotedSessionId,
        Dates = p.ProposedDates
            .OrderBy(d => d.ProposedDate)
            .Select(d => new ProposalDateResponse
            {
                Id = d.Id,
                ProposedDate = d.ProposedDate,
                Slot = d.Slot,
                Status = d.Status,
                SessionId = d.SessionId,
                CanCount = d.Votes.Count(v => v.CanAttend),
                CannotCount = d.Votes.Count(v => !v.CanAttend),
                CurrentUserVote = d.Votes.FirstOrDefault(v => v.UserId == currentUserId)?.CanAttend,
            })
            .ToList()
    };
}
