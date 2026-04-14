namespace Messages.Proposals.Out;

public class ProposalResponse
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public required string Title { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public long? PromotedSessionId { get; set; }
    public List<ProposalDateResponse> Dates { get; set; } = [];
}
