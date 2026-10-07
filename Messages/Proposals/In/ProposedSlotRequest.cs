namespace Messages.Proposals.In;

public class ProposedSlotRequest
{
    public DateTime Date { get; set; }
    public string? Slot { get; set; } // morning | afternoon | null (free time)
}
