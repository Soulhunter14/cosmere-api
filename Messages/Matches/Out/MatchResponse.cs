namespace Messages.Matches.Out;

public class MatchResponse
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<SceneResponse> Scenes { get; set; } = [];
}

public class SceneResponse
{
    public long Id { get; set; }
    public required string Description { get; set; }
    public int OrderIndex { get; set; }
}
