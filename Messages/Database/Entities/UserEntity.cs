namespace Messages.Database.Entities;

public class UserEntity
{
    public long Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string DisplayName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CampaignMemberEntity> CampaignMemberships { get; set; } = [];
}
