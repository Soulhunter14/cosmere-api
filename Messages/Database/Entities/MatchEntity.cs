namespace Messages.Database.Entities;

public class MatchEntity
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public CampaignEntity Campaign { get; set; } = null!;
    public ICollection<SceneEntity> Scenes { get; set; } = [];
}
