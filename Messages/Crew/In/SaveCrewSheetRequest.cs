using System.Text.Json;

namespace Messages.Crew.In;

public class SaveCrewSheetRequest
{
    /// <summary>The whole crew sheet. It must be a JSON object; the server stores it without reading inside it.</summary>
    public JsonElement State { get; set; }
    /// <summary>The GM's private part, a JSON object. Absent or <c>null</c> = keep the stored one.</summary>
    public JsonElement? GmState { get; set; }
    /// <summary>The version the client read (<c>CrewSheetResponse.Version</c>); 0 = the sheet has never been saved.</summary>
    public int Version { get; set; }
}
