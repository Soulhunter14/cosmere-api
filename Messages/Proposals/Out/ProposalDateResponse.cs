namespace Messages.Proposals.Out;

public class ProposalDateResponse
{
    public long Id { get; set; }
    public DateTime ProposedDate { get; set; }
    public int CanCount { get; set; }
    public int CannotCount { get; set; }
    public bool? CurrentUserVote { get; set; } // null = not voted, true = Can, false = Cannot
}
