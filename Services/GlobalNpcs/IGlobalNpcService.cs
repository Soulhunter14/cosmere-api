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
    Task<(byte[] Data, string ContentType)> GetBookImageAsync(long id, string kind, long? campaignId, long userId);
    Task<GlobalNpcResponse> SaveBookImageAsync(long id, string kind, byte[] data, long? campaignId, long userId);
    Task DeleteBookImageAsync(long id, string kind, long? campaignId, long userId);
}
