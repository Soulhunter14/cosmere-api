using System.Text.Json;

namespace Messages.GmScreen.Out;

/// <summary>What the players read of the screen: the public document the GM projected («La mesa»), nothing else.</summary>
public class MesaResponse
{
    /// <summary>The public document, a JSON object (<c>{}</c> when nothing was projected yet).</summary>
    public JsonElement State { get; set; }
    /// <summary>The version of the screen it was saved with (the number the <c>MesaUpdated</c> event carries). 0 = never saved.</summary>
    public int Version { get; set; }
    /// <summary>When the document last changed; <c>null</c> = never.</summary>
    public DateTime? UpdatedAt { get; set; }
}
