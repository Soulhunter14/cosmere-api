using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.Metas.In;
using Messages.Metas.Out;
using Microsoft.EntityFrameworkCore;
using Services.Worlds;

namespace Services.Metas;

public class MetaService(CosmereContext db, IWorldRulesProvider reglas) : IMetaService
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

        // Concluir la meta con cualquier tipo de conclusión da su recompensa (L.284 / PDF 290; Q20): la aplica el mundo de la
        // campaña (Stormlight no hace nada). La conclusión y el efecto del mundo se guardan juntos, en una sola transacción.
        var world = await GetWorldRulesAsync(campaignId);
        var character = await db.Characters.FirstAsync(c => c.Id == characterId);
        world.AlConcluirMeta(character, meta);

        await db.SaveChangesAsync();
        return MapToResponse(meta);
    }

    public async Task DeleteMetaAsync(long metaId, long characterId, long campaignId, long userId)
    {
        var meta = await GetMetaOrThrowAsync(metaId, characterId, campaignId, userId);
        db.Metas.Remove(meta);

        // Lo que el mundo hubiera enlazado a la meta queda desenlazado (Stormlight no hace nada); se guarda junto al borrado.
        var world = await GetWorldRulesAsync(campaignId);
        var character = await db.Characters.FirstAsync(c => c.Id == characterId);
        world.AlBorrarMeta(character, meta);

        await db.SaveChangesAsync();
    }

    /// <summary>Reglas del mundo de la campaña; un mundo nulo o desconocido cae a Stormlight.</summary>
    private async Task<IWorldRules> GetWorldRulesAsync(long campaignId) =>
        reglas.Get(await db.Campaigns.AsNoTracking()
            .Where(c => c.Id == campaignId)
            .Select(c => c.World)
            .FirstOrDefaultAsync());

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
