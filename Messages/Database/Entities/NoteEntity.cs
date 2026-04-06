namespace Messages.Database.Entities;

public class NoteEntity
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public long FromUserId { get; set; }
    public long ToUserId { get; set; }
    public required string Content { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CampaignEntity Campaign { get; set; } = null!;
    public UserEntity FromUser { get; set; } = null!;
    public UserEntity ToUser { get; set; } = null!;
}
