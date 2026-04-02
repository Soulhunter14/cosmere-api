using Cross.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

[Authorize]
public class CampaignHub : Hub
{
    public async Task JoinCampaign(string campaignId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"campaign-{campaignId}");
    }

    public async Task LeaveCampaign(string campaignId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"campaign-{campaignId}");
    }

    // Server-side helpers called by services to broadcast updates
    public static string CampaignGroup(long campaignId) => $"campaign-{campaignId}";
}
