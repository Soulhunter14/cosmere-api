namespace Messages.Crew.In;

/// <summary>A character takes a lot out of the crew's store: its units go to their inventory and the lot closes, together.</summary>
public class SacarAlmacenRequest
{
    /// <summary>The character who takes it, and whose inventory it goes to.</summary>
    public long CharacterId { get; set; }
    /// <summary>Ledger text (the web writes it); at most 200 characters.</summary>
    public string Motivo { get; set; } = "";
}
