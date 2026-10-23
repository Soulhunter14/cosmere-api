using System.Text.Json;

namespace Messages.GmScreen.In;

public class SaveGmScreenRequest
{
    /// <summary>The whole document of the screen. It must be a JSON object; the server stores it without reading inside it.</summary>
    public JsonElement State { get; set; }
    /// <summary>The version the client read (<c>GmScreenResponse.Version</c>); 0 = the campaign has never been saved.</summary>
    public int Version { get; set; }
    /// <summary>
    /// The public document the players read («La mesa»), a JSON object derived from the screen. Optional: when absent (or
    /// <c>null</c>) the stored one is left untouched.
    /// </summary>
    public JsonElement? Mesa { get; set; }
}
