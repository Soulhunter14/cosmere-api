using System.Text.Json;

namespace Messages.GmScreen.Out;

public class GmScreenResponse
{
    /// <summary>The whole document of the screen, a JSON object (<c>{}</c> when it has never been saved).</summary>
    public JsonElement State { get; set; }
    /// <summary>Optimistic concurrency token: send it back on the next save. 0 = never saved.</summary>
    public int Version { get; set; }
    /// <summary>When the document was last saved; <c>null</c> = never saved.</summary>
    public DateTime? UpdatedAt { get; set; }
}
