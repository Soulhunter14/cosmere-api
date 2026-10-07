namespace Messages.Proposals.In;

public class CreateProposalRequest
{
    public required string Title { get; set; }
    public string Notes { get; set; } = string.Empty;
    /// <summary>Plain dates without slot. Kept for older clients; ignored when <see cref="ProposedSlots"/> is sent.</summary>
    public List<DateTime> ProposedDates { get; set; } = [];
    public List<ProposedSlotRequest>? ProposedSlots { get; set; }
}
