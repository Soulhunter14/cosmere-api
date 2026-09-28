namespace Messages.DiceRolls.In;

public class CreateDiceRollRequest
{
    public string RollType { get; set; } = string.Empty;
    public string RollData { get; set; } = "{}";
    public string RollLabel { get; set; } = string.Empty;
    /// <summary>Null = rolling as the user. Set when GM impersonates a character.</summary>
    public string? CharacterName { get; set; }
}
