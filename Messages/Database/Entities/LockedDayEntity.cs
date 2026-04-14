namespace Messages.Database.Entities;

public class LockedDayEntity
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public long UserId { get; set; }
    public DateOnly Date { get; set; }
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CampaignEntity Campaign { get; set; } = null!;
    public UserEntity User { get; set; } = null!;
}
