namespace Messages.Database.Entities;

public class ProposalDateEntity
{
    public long Id { get; set; }
    public long ProposalId { get; set; }
    public DateTime ProposedDate { get; set; }
    public string? Slot { get; set; } // morning | afternoon | null (free time)
    public string Status { get; set; } = "Pending"; // Pending | Accepted | Rejected
    public long? SessionId { get; set; }

    public SessionProposalEntity Proposal { get; set; } = null!;
    public SessionEntity? Session { get; set; }
    public ICollection<ProposalVoteEntity> Votes { get; set; } = [];
}
