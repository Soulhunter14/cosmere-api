using Messages.GlobalNpcs.Out;

namespace Services.GlobalNpcs;

/// <summary>The world's catalog of adversaries, read-only (they come in migrations); only their book images are written.</summary>
public interface IGlobalNpcService
{
    Task<List<GlobalNpcResponse>> GetAllAsync(long? campaignId, long userId);
    Task<GlobalNpcResponse> GetByIdAsync(long id);
    Task<(byte[] Data, string ContentType)> GetBookImageAsync(long id, string kind, long? campaignId, long userId);
    Task<GlobalNpcResponse> SaveBookImageAsync(long id, string kind, byte[] data, long? campaignId, long userId);
    Task DeleteBookImageAsync(long id, string kind, long? campaignId, long userId);
}
