using Messages.GlobalNpcs.In;
using Messages.GlobalNpcs.Out;

namespace Services.GlobalNpcs;

public interface IGlobalNpcService
{
    Task<List<GlobalNpcResponse>> GetAllAsync();
    Task<GlobalNpcResponse> GetByIdAsync(long id);
    Task<GlobalNpcResponse> CreateAsync(GlobalNpcRequest request);
    Task<GlobalNpcResponse> UpdateAsync(long id, GlobalNpcRequest request);
    Task DeleteAsync(long id);
}
