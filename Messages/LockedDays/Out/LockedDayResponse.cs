namespace Messages.LockedDays.Out;

public class LockedDayResponse
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public long UserId { get; set; }
    public string UserDisplayName { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty; // "yyyy-MM-dd"
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
