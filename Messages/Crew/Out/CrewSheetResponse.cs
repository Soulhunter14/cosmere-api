using System.Text.Json;

namespace Messages.Crew.Out;

public class CrewSheetResponse
{
    /// <summary>The whole crew sheet, a JSON object (<c>{}</c> when it has never been saved).</summary>
    public JsonElement State { get; set; }
    /// <summary>The GM's private part (<c>{}</c> when empty); <c>null</c> for a player.</summary>
    public JsonElement? GmState { get; set; }
    /// <summary>Players who may read the sheet; <c>null</c> for a player.</summary>
    public List<long>? SharedWith { get; set; }
    /// <summary>Optimistic concurrency token: send it back on the next save. 0 = never saved.</summary>
    public int Version { get; set; }
    /// <summary>When the sheet was last saved; <c>null</c> = never saved.</summary>
    public DateTime? UpdatedAt { get; set; }
}
