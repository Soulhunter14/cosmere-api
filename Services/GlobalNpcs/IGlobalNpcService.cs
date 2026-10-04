using Messages.GlobalNpcs.In;
using Messages.GlobalNpcs.Out;

namespace Services.GlobalNpcs;

public interface IGlobalNpcService
{
    Task<List<GlobalNpcResponse>> GetAllAsync(long? campaignId, long userId);
    Task<GlobalNpcResponse> GetByIdAsync(long id);
    Task<GlobalNpcResponse> CreateAsync(GlobalNpcRequest request, long? campaignId, long userId);
    Task<GlobalNpcResponse> UpdateAsync(long id, GlobalNpcRequest request, long? campaignId, long userId);
    Task DeleteAsync(long id, long? campaignId, long userId);
}
