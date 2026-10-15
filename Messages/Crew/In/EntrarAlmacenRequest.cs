using System.Text.Json;

namespace Messages.Crew.In;

/// <summary>A character puts goods of their inventory in the crew's store: they leave the inventory and enter the store together.</summary>
public class EntrarAlmacenRequest
{
    /// <summary>The character whose inventory they leave.</summary>
    public long CharacterId { get; set; }
    /// <summary>The inventory list they come from: "weapons", "armor" or "equipment".</summary>
    public string Lista { get; set; } = "";
    /// <summary>Name of the item as the inventory keeps it (one entry per unit).</summary>
    public string Nombre { get; set; } = "";
    /// <summary>How many units, 1..999; the inventory must carry that many.</summary>
    public int Cantidad { get; set; }
    /// <summary>
    /// The lot as the web builds it (id, conservation, value, price, notes…). The server fixes who brought it, when, its name, quantity
    /// and list, and that it is on sale only with a value and a price.
    /// </summary>
    public JsonElement Lote { get; set; }
    /// <summary>Ledger text of the entry (the web writes it: the money's format is the world's); at most 200 characters.</summary>
    public string Motivo { get; set; } = "";
    /// <summary>Ledger text when it also goes on sale; at most 200 characters.</summary>
    public string? MotivoVenta { get; set; }
}
