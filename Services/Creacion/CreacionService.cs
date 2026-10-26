using Infrastructure.Data;
using Messages.Creacion.Out;
using Messages.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Services.Characters;
using Services.Campaigns;
using Services.Worlds;

namespace Services.Creacion;

/// <summary>
/// Creación guiada de personajes (ver <c>CreacionGuiada</c>). El director libera pasos (<c>Campaign.CreacionPaso</c>), decide qué pasos
/// respondidos se pueden cambiar (<c>Campaign.CreacionCambiables</c>), crea fichas asignadas a los jugadores (en creación, con nombre
/// provisional) y las da por creadas; los jugadores rellenan su ficha con el <c>PUT</c> de personajes, que vigila los pasos liberados y
/// los cambiables. Solo mundos con pasos (<c>IWorldRules.PasosCreacion</c>).
/// </summary>
public class CreacionService(CosmereContext db, IWorldRulesProvider mundos) : ICreacionService
{
    public async Task<CreacionResponse> GetAsync(long campaignId, long userId)
    {
        var member = await db.CampaignMembers.AsNoTracking()
            .FirstOrDefaultAsync(m => m.CampaignId == campaignId && m.UserId == userId)
            ?? throw new KeyNotFoundException("Campaign not found.");
        return await BuildAsync(campaignId, member.Role == "gm" ? null : userId);
    }

    public async Task<CreacionResponse> SetPasoAsync(long campaignId, int paso, long userId)
    {
        var campaign = await GetGmCampaignAsync(campaignId, userId);
        var pasos = PasosDe(campaign);
        if (paso < 0 || paso > pasos.Count)
            throw new ArgumentException($"Invalid paso: {paso} (0 to {pasos.Count}).");
        campaign.CreacionPaso = paso;
        await db.SaveChangesAsync();
        return await BuildAsync(campaignId, null);
    }

    public async Task<CreacionResponse> SetCambiableAsync(long campaignId, string paso, bool permitido, long userId)
    {
        var campaign = await GetGmCampaignAsync(campaignId, userId);
        var pasos = PasosDe(campaign);
        if (!pasos.Contains(paso))
            throw new ArgumentException($"Invalid paso: '{paso}'.");
        // Lista nueva (no la misma instancia) para que EF detecte el cambio; en el orden de los pasos del mundo.
        var actuales = campaign.CreacionCambiables;
        campaign.CreacionCambiables = pasos.Where(p => p == paso ? permitido : actuales.Contains(p)).ToList();
        await db.SaveChangesAsync();
        return await BuildAsync(campaignId, null);
    }

    public async Task<CreacionResponse> AddPersonajeAsync(long campaignId, long ownerId, long userId)
    {
        var campaign = await GetGmCampaignAsync(campaignId, userId);
        PasosDe(campaign);
        var esMiembro = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == ownerId);
        if (!esMiembro) throw new KeyNotFoundException("Assigned player is not a campaign member.");
        var jugador = await db.Set<UserEntity>().AsNoTracking().Where(u => u.Id == ownerId).Select(u => u.DisplayName).FirstAsync();

        // Nivel 0 con los seis atributos en 1, como «El primer paso» y la sesión 0 «La caída»: la hoja se construye jugando. El nombre
        // es provisional: ponerlo es el primer paso del jugador.
        var personaje = new CharacterEntity
        {
            CampaignId = campaignId,
            OwnerId = ownerId,
            Name = CreacionGuiada.NombreProvisional + jugador,
            PlayerName = jugador,
            Level = 0,
            Ascendencia = string.Empty,
            CaminoHeroico = string.Empty,
            CaminoRadiante = string.Empty,
            CaminoMetal = string.Empty,
            CaminoInicial = string.Empty,
            Fuerza = 1, Velocidad = 1, Intelecto = 1, Voluntad = 1, Discernimiento = 1, Presencia = 1,
            IsNpc = false,
            CreacionCompletada = false,
        };
        db.Characters.Add(personaje);
        await db.SaveChangesAsync();
        return await BuildAsync(campaignId, null);
    }

    public async Task<CreacionResponse> CompletarAsync(long campaignId, long characterId, bool completada, long userId)
    {
        await GetGmCampaignAsync(campaignId, userId);
        var personaje = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == characterId && c.CampaignId == campaignId && !c.IsNpc)
            ?? throw new KeyNotFoundException("Character not found.");
        personaje.CreacionCompletada = completada;
        personaje.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return await BuildAsync(campaignId, null);
    }

    /// <summary>El estado: todos los personajes (director) o solo los de <paramref name="soloDe"/> (jugador).</summary>
    private async Task<CreacionResponse> BuildAsync(long campaignId, long? soloDe)
    {
        var campaign = await db.Campaigns.AsNoTracking().FirstAsync(c => c.Id == campaignId);
        var pasos = mundos.Get(campaign.World).PasosCreacion;

        var consulta = db.Characters.AsNoTracking().Where(c => c.CampaignId == campaignId && !c.IsNpc);
        if (soloDe is not null) consulta = consulta.Where(c => c.OwnerId == soloDe);
        var personajes = await consulta.OrderBy(c => c.Id).ToListAsync();

        var ownerIds = personajes.Where(c => c.OwnerId is not null).Select(c => c.OwnerId!.Value).Distinct().ToList();
        var nombres = ownerIds.Count == 0
            ? new Dictionary<long, string>()
            : await db.Set<UserEntity>().AsNoTracking().Where(u => ownerIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.DisplayName);

        return new CreacionResponse
        {
            Paso = campaign.CreacionPaso,
            Pasos = [.. pasos],
            Iniciada = campaign.IniciadaEn is not null,
            Cambiables = [.. campaign.CreacionCambiables],
            Personajes = personajes.Select(c => new CreacionPersonajeResponse
            {
                Id = c.Id,
                Name = c.Name,
                OwnerId = c.OwnerId,
                OwnerName = c.OwnerId is not null && nombres.TryGetValue(c.OwnerId.Value, out var n) ? n : null,
                Legado = c.Legado,
                Ascendencia = c.Ascendencia,
                LegadoRespuestas = c.LegadoRespuestas,
                CreacionDatos = CharacterJson.ParseCreacionDatos(c.CreacionDatos),
                Completada = c.CreacionCompletada,
                UpdatedAt = c.UpdatedAt,
            }).ToList(),
        };
    }

    private IReadOnlyList<string> PasosDe(CampaignEntity campaign)
    {
        var pasos = mundos.Get(campaign.World).PasosCreacion;
        if (pasos.Count == 0) throw new ArgumentException("This world has no guided character creation.");
        return pasos;
    }

    /// <summary>El director es el miembro con rol «gm» (como en <c>CharacterService</c> y <c>GmScreenService</c>), no solo quien creó la campaña.</summary>
    private async Task<CampaignEntity> GetGmCampaignAsync(long campaignId, long userId)
    {
        var esGm = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId && m.Role == "gm");
        if (!esGm)
            throw new UnauthorizedAccessException("Only the GM can perform this action.");
        return await db.Campaigns.FirstOrDefaultAsync(c => c.Id == campaignId)
            ?? throw new KeyNotFoundException("Campaign not found.");
    }
}
