namespace Messages.Database.Entities;

public class SideQuestEntity
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public required string Name { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Acts { get; set; } = [];
    public List<string> Rewards { get; set; } = [];
    public List<string> Benefits { get; set; } = [];
    public string Notes { get; set; } = string.Empty;
    public bool Started { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CampaignEntity Campaign { get; set; } = null!;
}
