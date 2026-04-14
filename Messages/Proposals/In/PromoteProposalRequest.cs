namespace Messages.Proposals.In;

public class PromoteProposalRequest
{
    public long ProposalDateId { get; set; }
    public required string Title { get; set; }
    public string Location { get; set; } = string.Empty;
}
