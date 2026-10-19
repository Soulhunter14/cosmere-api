using Messages.Worlds;

namespace Messages.Calendar.Out;

/// <summary>A session or a pending proposed slot of another campaign of the user, shown read-only in the calendar.</summary>
public class OtherCampaignEventResponse
{
    public long CampaignId { get; set; }
    public required string CampaignName { get; set; }
    public string World { get; set; } = WorldIds.Stormlight;
    /// <summary>Role of the user in that campaign: gm | player</summary>
    public required string Role { get; set; }
    /// <summary>session | proposal</summary>
    public required string Kind { get; set; }
    /// <summary>Session id, or proposal date id for a proposed slot</summary>
    public long Id { get; set; }
    /// <summary>Session title, or proposal title</summary>
    public required string Title { get; set; }
    public DateTime Date { get; set; }
    /// <summary>morning | afternoon | null (free time); always null for sessions</summary>
    public string? Slot { get; set; }
    public string Location { get; set; } = string.Empty;
}
