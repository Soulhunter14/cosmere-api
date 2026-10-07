namespace Messages.Proposals;

/// <summary>Fixed slots of a proposed day. The start time of each slot is chosen by the client (morning 10:00, afternoon 15:00).</summary>
public static class ProposalSlots
{
    public const string Morning = "morning";
    public const string Afternoon = "afternoon";

    public static bool IsValid(string? slot) => slot is null or Morning or Afternoon;
}
