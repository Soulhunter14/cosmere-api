using Messages.SideQuests.In;
using Messages.SideQuests.Out;

namespace Services.SideQuests;

public interface ISideQuestService
{
    Task<List<SideQuestResponse>> GetSideQuestsAsync(long campaignId, long userId);
    Task<SideQuestResponse> GetSideQuestAsync(long sideQuestId, long campaignId, long userId);
    Task<SideQuestResponse> CreateSideQuestAsync(long campaignId, CreateSideQuestRequest request, long userId);
    Task<SideQuestResponse> UpdateSideQuestAsync(long sideQuestId, long campaignId, UpdateSideQuestRequest request, long userId);
    Task DeleteSideQuestAsync(long sideQuestId, long campaignId, long userId);
}
