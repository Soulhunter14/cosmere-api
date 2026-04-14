namespace Messages.Database.Entities;

public class ProposalVoteEntity
{
    public long Id { get; set; }
    public long ProposalDateId { get; set; }
    public long UserId { get; set; }
    public bool CanAttend { get; set; }
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;

    public ProposalDateEntity ProposalDate { get; set; } = null!;
    public UserEntity User { get; set; } = null!;
}
