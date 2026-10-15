using Messages.Crew.In;
using Messages.Crew.Out;

namespace Services.Crew;

public interface ICrewSheetService
{
    /// <summary>
    /// The crew sheet, or an empty one (`{}`, version 0) when the campaign has none yet. The GM gets everything; a player gets the
    /// sheet only when the GM has shared it with them (without the GM part or the sharing list), otherwise 403.
    /// </summary>
    Task<CrewSheetResponse> GetAsync(long campaignId, long userId);
    /// <summary>
    /// Saves the sheet when `request.Version` is the stored version; otherwise nothing is written and the stored sheet comes back as a
    /// conflict. The GM also writes the GM part; a player the sheet is shared with writes the sheet only (never the GM part, never
    /// creating it).
    /// </summary>
    Task<CrewSheetSaveResult> SaveAsync(long campaignId, SaveCrewSheetRequest request, long userId);
    /// <summary>Replaces the players who may read the sheet. Only players of the campaign are accepted; it does not change the version. GM only.</summary>
    Task<CrewSheetResponse> ShareAsync(long campaignId, ShareCrewSheetRequest request, long userId);
    /// <summary>
    /// A character gives money to the crew: it leaves their purse (the world's money key in `Recursos`) and enters the treasury, with its
    /// movement in the ledger, in one transaction. The GM for any character; a player for their own, when the sheet is shared with them.
    /// </summary>
    Task<CrewSheetResponse> AportarAsync(long campaignId, AportarCrewRequest request, long userId);
    /// <summary>
    /// A character puts goods of their inventory in the crew's store: the units leave the inventory list (the worn armor comes off when
    /// none is left) and the lot enters the store, with its movement in the ledger, in one transaction. The GM for any character; a
    /// player for their own, when the sheet is shared with them.
    /// </summary>
    Task<CrewSheetResponse> EntrarAlmacenAsync(long campaignId, EntrarAlmacenRequest request, long userId);
    /// <summary>
    /// A character takes a lot out of the crew's store: it closes («retirada») and its units enter the character's inventory, with its
    /// movement in the ledger, in one transaction. The GM for any character; a player for their own, when the sheet is shared with them.
    /// </summary>
    Task<CrewSheetResponse> SacarAlmacenAsync(long campaignId, string loteId, SacarAlmacenRequest request, long userId);
}

/// <summary>`Saved` = written (Sheet is the new state); otherwise Sheet is the one stored on the server.</summary>
public record CrewSheetSaveResult(bool Saved, CrewSheetResponse Sheet);
