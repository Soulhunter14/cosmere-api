namespace Messages.Database.Entities;

public class NpcNoteEntity
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public long AuthorId { get; set; }
    public string NpcName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsShared { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public CampaignEntity Campaign { get; set; } = null!;
    public UserEntity Author { get; set; } = null!;
}
