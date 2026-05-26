namespace Messages.Database.Entities;

public class DiaryEntryEntity
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public int Number { get; set; }
    public required string Title { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Preview { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<string> Participants { get; set; } = [];
    public string MentionsJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public CampaignEntity Campaign { get; set; } = null!;
}
