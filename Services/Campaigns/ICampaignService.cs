using Messages.Campaigns.In;
using Messages.Campaigns.Out;

namespace Services.Campaigns;

public interface ICampaignService
{
    Task<List<CampaignResponse>> GetUserCampaignsAsync(long userId);
    Task<CampaignDetailResponse> GetCampaignDetailAsync(long campaignId, long userId);
    Task<CampaignDetailResponse> CreateCampaignAsync(CreateCampaignRequest request, long userId);
    Task DeleteCampaignAsync(long campaignId, long userId);
    Task<CampaignResponse> JoinCampaignAsync(JoinCampaignRequest request, long userId);
    Task UpdateInviteAsync(long campaignId, UpdateInviteRequest request, long userId);
    Task<string> RegenerateInviteCodeAsync(long campaignId, long userId);
}
