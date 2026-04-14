namespace Messages.Database.Entities;

public class ProposalDateEntity
{
    public long Id { get; set; }
    public long ProposalId { get; set; }
    public DateTime ProposedDate { get; set; }

    public SessionProposalEntity Proposal { get; set; } = null!;
    public ICollection<ProposalVoteEntity> Votes { get; set; } = [];
}
