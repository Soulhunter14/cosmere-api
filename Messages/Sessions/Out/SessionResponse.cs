namespace Messages.Sessions.Out;

public class SessionResponse
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public required string Title { get; set; }
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
