namespace Messages.Database.Entities;

/// <summary>
/// «La banda»: the crew sheet of one campaign (scale, treasury, refuges, workers, contacts, ledger) as ONE JSON document owned by
/// the web client (`src/pages/banda/estado.ts`), plus the GM's private part. One row per campaign, created on the first write.
/// Players do not see it unless the GM shares it with them (<see cref="SharedWith"/>), and then only to read it.
/// </summary>
public class CrewSheetEntity
{
    public long CampaignId { get; set; }
    /// <summary>The crew sheet, a JSON object stored as text; the server never reads inside it.</summary>
    public string State { get; set; } = "{}";
    /// <summary>The GM's private part (notes, secrets), a JSON object stored as text; only the GM receives it.</summary>
    public string GmState { get; set; } = "{}";
    /// <summary>User ids of the players who may read the sheet (never edit it). Empty = only the GM.</summary>
    public List<long> SharedWith { get; set; } = [];
    /// <summary>Optimistic concurrency of the sheet: a save must send the version it read; every save bumps it (sharing does not).</summary>
    public int Version { get; set; }
    /// <summary>Last save of the sheet.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public CampaignEntity Campaign { get; set; } = null!;
}
