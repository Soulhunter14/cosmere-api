namespace Messages.Crew.In;

public class ShareCrewSheetRequest
{
    /// <summary>User ids of the players who may read the crew sheet. It replaces the stored list; empty = only the GM.</summary>
    public List<long> UserIds { get; set; } = [];
}
