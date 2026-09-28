namespace Messages.DiceRolls.Out;

public class DiceRollResponse
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public long UserId { get; set; }
    public string UserDisplayName { get; set; } = string.Empty;
    public string RollType { get; set; } = string.Empty;
    public string RollData { get; set; } = "{}";
    public string RollLabel { get; set; } = string.Empty;
    public string? CharacterName { get; set; }
    public DateTime CreatedAt { get; set; }
}
