using Messages.GmScreen.In;
using Messages.GmScreen.Out;

namespace Services.GmScreen;

public interface IGmScreenService
{
    /// <summary>The saved document, or an empty one (`{}`, version 0) when the campaign has none yet. GM only.</summary>
    Task<GmScreenResponse> GetAsync(long campaignId, long userId);
    /// <summary>Saves when `request.Version` is the stored version; otherwise nothing is written and the stored document comes back as a conflict. GM only.</summary>
    Task<GmScreenSaveResult> SaveAsync(long campaignId, SaveGmScreenRequest request, long userId);
}

/// <summary>`Saved` = written (Screen is the new state); otherwise Screen is the document stored on the server.</summary>
public record GmScreenSaveResult(bool Saved, GmScreenResponse Screen);
