namespace Messages.Database.Entities;

/// <summary>
/// «Pantalla del director»: the GM's table state of one campaign (encounter tracker, current scene, session log, progress marks)
/// as ONE JSON document owned by the web client (`src/pages/pantalla/estado.ts`). One row per campaign, created on the first save.
/// </summary>
public class GmScreenEntity
{
    public long CampaignId { get; set; }
    /// <summary>A JSON object stored as text; the server never reads inside it.</summary>
    public string State { get; set; } = "{}";
    /// <summary>Optimistic concurrency: a save must send the version it read; every save bumps it.</summary>
    public int Version { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public CampaignEntity Campaign { get; set; } = null!;
}
