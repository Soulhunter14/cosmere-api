namespace Messages.Database.Entities;

public class SessionProposalEntity
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public required string Title { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending | Promoted | Rejected
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public long? PromotedSessionId { get; set; }

    public CampaignEntity Campaign { get; set; } = null!;
    public SessionEntity? PromotedSession { get; set; }
    public ICollection<ProposalDateEntity> ProposedDates { get; set; } = [];
}
