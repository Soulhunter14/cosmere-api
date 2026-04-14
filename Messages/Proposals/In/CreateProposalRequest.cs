namespace Messages.Proposals.In;

public class CreateProposalRequest
{
    public required string Title { get; set; }
    public string Notes { get; set; } = string.Empty;
    public required List<DateTime> ProposedDates { get; set; }
}
