namespace Messages.SideQuests.Out;

public class SideQuestResponse
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public required string Name { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Acts { get; set; } = [];
    public List<string> Rewards { get; set; } = [];
    public List<string> Benefits { get; set; } = [];
    public string Notes { get; set; } = string.Empty; // only returned to GM
    public bool Started { get; set; }
    public DateTime CreatedAt { get; set; }
}
