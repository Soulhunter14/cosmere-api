using Messages.Npcs.In;
using Messages.Npcs.Out;

namespace Services.Npcs;

public interface INpcService
{
    Task<List<NpcResponse>> GetNpcsAsync(long campaignId, long userId);
    Task<NpcResponse> GetNpcAsync(long npcId, long campaignId, long userId);
    Task<NpcResponse> CreateNpcAsync(long campaignId, CreateNpcRequest request, long userId);
    Task<NpcResponse> UpdateNpcAsync(long npcId, long campaignId, UpdateNpcRequest request, long userId);
    Task<NpcResponse> CloneNpcAsync(long npcId, long campaignId, long userId);
    Task ToggleVisibilityAsync(long npcId, long campaignId, ToggleNpcVisibilityRequest request, long userId);
    Task DeleteNpcAsync(long npcId, long campaignId, long userId);
}
