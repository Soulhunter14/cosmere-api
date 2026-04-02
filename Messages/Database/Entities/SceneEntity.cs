namespace Messages.Database.Entities;

public class SceneEntity
{
    public long Id { get; set; }
    public long MatchId { get; set; }
    public required string Description { get; set; }
    public int OrderIndex { get; set; }

    public MatchEntity Match { get; set; } = null!;
}
