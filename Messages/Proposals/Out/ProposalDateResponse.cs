namespace Messages.Proposals.Out;

public class ProposalDateResponse
{
    public long Id { get; set; }
    public DateTime ProposedDate { get; set; }
    public string? Slot { get; set; } // morning | afternoon | null (free time)
    public string Status { get; set; } = "Pending"; // Pending | Accepted | Rejected
    public long? SessionId { get; set; }
    public int CanCount { get; set; }
    public int CannotCount { get; set; }
    public bool? CurrentUserVote { get; set; } // null = not voted, true = Can, false = Cannot
    /// <summary>Players of the campaign right now (the votes expected for this slot); CanCount/CannotCount only count players</summary>
    public int PlayerCount { get; set; }
    /// <summary>Players who have not voted this slot yet</summary>
    public List<long> PendingUserIds { get; set; } = [];
}
