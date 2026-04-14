using Messages.Metas.In;
using Messages.Metas.Out;

namespace Services.Metas;

public interface IMetaService
{
    Task<List<MetaResponse>> GetMetasAsync(long characterId, long campaignId, long userId);
    Task<MetaResponse> CreateMetaAsync(long characterId, long campaignId, CreateMetaRequest request, long userId);
    Task<MetaResponse> UpdateMetaAsync(long metaId, long characterId, long campaignId, UpdateMetaRequest request, long userId);
    Task<MetaResponse> ConcludeMetaAsync(long metaId, long characterId, long campaignId, ConcludeMetaRequest request, long userId);
    Task DeleteMetaAsync(long metaId, long characterId, long campaignId, long userId);
}
