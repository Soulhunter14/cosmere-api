namespace Messages.Database.Entities;

public class CampaignMemberEntity
{
    public long CampaignId { get; set; }
    public long UserId { get; set; }
    public required string Role { get; set; } // "gm" | "player"
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public CampaignEntity Campaign { get; set; } = null!;
    public UserEntity User { get; set; } = null!;
}
