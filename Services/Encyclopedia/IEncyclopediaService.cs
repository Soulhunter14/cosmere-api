using Messages.Encyclopedia.In;
using Messages.Encyclopedia.Out;

namespace Services.Encyclopedia;

public interface IEncyclopediaService
{
    Task<List<EncyclopediaOriginalSummaryResponse>> ListAsync(long campaignId, long userId);
    Task<EncyclopediaTextResponse> GetTextAsync(long campaignId, string section, string key, long userId);
    Task<(byte[] Data, string Title)> GetPdfAsync(long campaignId, string section, string key, long userId);
    Task<EncyclopediaOriginalSummaryResponse> SaveTextAsync(long campaignId, string section, string key, SaveEncyclopediaTextRequest request, long userId);
    Task<EncyclopediaOriginalSummaryResponse> SavePdfAsync(long campaignId, string section, string key, string title, byte[] data, long userId);
    Task DeleteTextAsync(long campaignId, string section, string key, long userId);
    Task DeletePdfAsync(long campaignId, string section, string key, long userId);
}
