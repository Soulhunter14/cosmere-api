namespace Messages.Crew.In;

public class AportarCrewRequest
{
    /// <summary>The character who gives the money: it leaves their purse and enters the crew's treasury.</summary>
    public long CharacterId { get; set; }
    /// <summary>Amount in the world's currency; more than 0, at most 2 decimals.</summary>
    public decimal Cantidad { get; set; }
    /// <summary>Why, for the crew's ledger (at most 200 characters).</summary>
    public string Motivo { get; set; } = "";
}
