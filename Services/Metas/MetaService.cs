using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.Metas.In;
using Messages.Metas.Out;
using Microsoft.EntityFrameworkCore;

namespace Services.Metas;

public class MetaService(CosmereContext db) : IMetaService
{
    public async Task<List<MetaResponse>> GetMetasAsync(long characterId, long campaignId, long userId)
    {
        await EnsureAccessAsync(characterId, campaignId, userId);

        return await db.Metas
            .Where(m => m.CharacterId == characterId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => MapToResponse(m))
            .ToListAsync();
    }

    public async Task<MetaResponse> CreateMetaAsync(long characterId, long campaignId, CreateMetaRequest request, long userId)
    {
        await EnsureAccessAsync(characterId, campaignId, userId);

        var meta = new MetaEntity
        {
            CharacterId = characterId,
            Titulo = request.Titulo,
            Descripcion = request.Descripcion,
        };

        db.Metas.Add(meta);
        await db.SaveChangesAsync();
        return MapToResponse(meta);
    }

    public async Task<MetaResponse> UpdateMetaAsync(long metaId, long characterId, long campaignId, UpdateMetaRequest request, long userId)
    {
        var meta = await GetMetaOrThrowAsync(metaId, characterId, campaignId, userId);

        if (request.Hitos < 0 || request.Hitos > 3)
            throw new ArgumentException("Hitos must be between 0 and 3.");

        meta.Titulo = request.Titulo;
        meta.Descripcion = request.Descripcion;
        meta.Hitos = request.Hitos;

        await db.SaveChangesAsync();
        return MapToResponse(meta);
    }

    public async Task<MetaResponse> ConcludeMetaAsync(long metaId, long characterId, long campaignId, ConcludeMetaRequest request, long userId)
    {
        var meta = await GetMetaOrThrowAsync(metaId, characterId, campaignId, userId);

        string[] validTypes = ["exito", "crecimiento", "fracaso"];
        if (!validTypes.Contains(request.TipoConclusion))
            throw new ArgumentException("TipoConclusion must be 'exito', 'crecimiento', or 'fracaso'.");

        meta.Estado = "concluida";
        meta.TipoConclusion = request.TipoConclusion;
        meta.NotasConclusion = request.NotasConclusion;

        await db.SaveChangesAsync();
        return MapToResponse(meta);
    }

    public async Task DeleteMetaAsync(long metaId, long characterId, long campaignId, long userId)
    {
        var meta = await GetMetaOrThrowAsync(metaId, characterId, campaignId, userId);
        db.Metas.Remove(meta);
        await db.SaveChangesAsync();
    }

    private async Task<MetaEntity> GetMetaOrThrowAsync(long metaId, long characterId, long campaignId, long userId)
    {
        await EnsureAccessAsync(characterId, campaignId, userId);

        return await db.Metas
            .FirstOrDefaultAsync(m => m.Id == metaId && m.CharacterId == characterId)
            ?? throw new KeyNotFoundException("Meta not found.");
    }

    private async Task EnsureAccessAsync(long characterId, long campaignId, long userId)
    {
        var isMember = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId);
        if (!isMember) throw new KeyNotFoundException("Campaign not found.");

        var isGm = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId && m.Role == "gm");

        var character = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == characterId && c.CampaignId == campaignId && !c.IsNpc)
            ?? throw new KeyNotFoundException("Character not found.");

        if (!isGm && character.OwnerId != userId)
            throw new UnauthorizedAccessException("You can only manage your own character's metas.");
    }

    private static MetaResponse MapToResponse(MetaEntity m) => new()
    {
        Id = m.Id, CharacterId = m.CharacterId, Titulo = m.Titulo,
        Descripcion = m.Descripcion, Hitos = m.Hitos, Estado = m.Estado,
        TipoConclusion = m.TipoConclusion, NotasConclusion = m.NotasConclusion,
        CreatedAt = m.CreatedAt,
    };
}
