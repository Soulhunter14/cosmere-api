using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.GlobalNpcs.In;
using Messages.GlobalNpcs.Out;
using Messages.Worlds;
using Microsoft.EntityFrameworkCore;

namespace Services.GlobalNpcs;

public class GlobalNpcService(CosmereContext db) : IGlobalNpcService
{
    public async Task<List<GlobalNpcResponse>> GetAllAsync(long? campaignId, long userId)
    {
        // The campaign's world and, in an era campaign, the adversaries of its era plus those of both eras (Era null)
        var (world, era) = await GetScopeAsync(campaignId, userId);
        return await db.GlobalNpcs
            .Where(n => n.World == world && (era == null || n.Era == null || n.Era == era))
            .OrderBy(n => n.Name).Select(n => Map(n)).ToListAsync();
    }

    public async Task<GlobalNpcResponse> GetByIdAsync(long id) =>
        Map(await db.GlobalNpcs.FindAsync(id) ?? throw new KeyNotFoundException("Global NPC not found."));

    public async Task<GlobalNpcResponse> CreateAsync(GlobalNpcRequest request, long? campaignId, long userId)
    {
        var (world, era) = await GetScopeAsync(campaignId, userId);
        var entity = FromRequest(request, world);
        entity.Era = era;
        db.GlobalNpcs.Add(entity);
        await db.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<GlobalNpcResponse> UpdateAsync(long id, GlobalNpcRequest request, long? campaignId, long userId)
    {
        var entity = await FindInWorldAsync(id, campaignId, userId);
        Apply(entity, request);
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Map(entity);
    }

    public async Task DeleteAsync(long id, long? campaignId, long userId)
    {
        var entity = await FindInWorldAsync(id, campaignId, userId);
        db.GlobalNpcs.Remove(entity);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// World and era (catalog number, <see cref="EraIds.Numero"/>) of the operation: the campaign's (the caller must belong to
    /// the campaign) or, without a campaign, Stormlight and no era (clients that predate worlds). Neither comes from the request body.
    /// </summary>
    private async Task<(string World, short? Era)> GetScopeAsync(long? campaignId, long userId)
    {
        if (campaignId is null) return (WorldIds.Stormlight, null);

        var campaign = await db.Campaigns.AsNoTracking()
            .Where(c => c.Id == campaignId)
            .Select(c => new { c.World, c.Era })
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Campaign not found.");

        var isMember = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId);
        if (!isMember) throw new UnauthorizedAccessException("You are not a member of this campaign.");

        return (campaign.World, EraIds.Numero(campaign.Era));
    }

    private async Task<string> GetWorldAsync(long? campaignId, long userId) => (await GetScopeAsync(campaignId, userId)).World;

    /// <summary>The NPC to write to; 404 if it does not exist or belongs to a different world than the operation's.</summary>
    private async Task<GlobalNpcEntity> FindInWorldAsync(long id, long? campaignId, long userId)
    {
        var world = await GetWorldAsync(campaignId, userId);
        var entity = await db.GlobalNpcs.FindAsync(id);
        if (entity is null || entity.World != world) throw new KeyNotFoundException("Global NPC not found.");
        return entity;
    }

    private static GlobalNpcEntity FromRequest(GlobalNpcRequest r, string world) => new()
    {
        World = world,
        Name = r.Name, Source = r.Source, Tipo = r.Tipo, Ascendencia = r.Ascendencia, Level = r.Level,
        Fuerza = r.Fuerza, Velocidad = r.Velocidad, Intelecto = r.Intelecto,
        Voluntad = r.Voluntad, Discernimiento = r.Discernimiento, Presencia = r.Presencia,
        MaxHealth = r.MaxHealth, MaxConcentration = r.MaxConcentration, MaxInvestiture = r.MaxInvestiture,
        Agilidad = r.Agilidad, ArmasLigeras = r.ArmasLigeras, ArmasPesadas = r.ArmasPesadas,
        Atletismo = r.Atletismo, Hurto = r.Hurto, Sigilo = r.Sigilo,
        Deduccion = r.Deduccion, Disciplina = r.Disciplina, Intimidacion = r.Intimidacion,
        Manufactura = r.Manufactura, Medicina = r.Medicina, Conocimiento = r.Conocimiento,
        Engano = r.Engano, Liderazgo = r.Liderazgo, Percepcion = r.Percepcion,
        Perspicacia = r.Perspicacia, Persuasion = r.Persuasion, Supervivencia = r.Supervivencia,
        Talentos = r.Talentos, Apariencia = r.Apariencia, Notas = r.Notas,
    };

    private static void Apply(GlobalNpcEntity e, GlobalNpcRequest r)
    {
        e.Name = r.Name; e.Source = r.Source; e.Tipo = r.Tipo; e.Ascendencia = r.Ascendencia; e.Level = r.Level;
        e.Fuerza = r.Fuerza; e.Velocidad = r.Velocidad; e.Intelecto = r.Intelecto;
        e.Voluntad = r.Voluntad; e.Discernimiento = r.Discernimiento; e.Presencia = r.Presencia;
        e.MaxHealth = r.MaxHealth; e.MaxConcentration = r.MaxConcentration; e.MaxInvestiture = r.MaxInvestiture;
        e.Agilidad = r.Agilidad; e.ArmasLigeras = r.ArmasLigeras; e.ArmasPesadas = r.ArmasPesadas;
        e.Atletismo = r.Atletismo; e.Hurto = r.Hurto; e.Sigilo = r.Sigilo;
        e.Deduccion = r.Deduccion; e.Disciplina = r.Disciplina; e.Intimidacion = r.Intimidacion;
        e.Manufactura = r.Manufactura; e.Medicina = r.Medicina; e.Conocimiento = r.Conocimiento;
        e.Engano = r.Engano; e.Liderazgo = r.Liderazgo; e.Percepcion = r.Percepcion;
        e.Perspicacia = r.Perspicacia; e.Persuasion = r.Persuasion; e.Supervivencia = r.Supervivencia;
        e.Talentos = r.Talentos; e.Apariencia = r.Apariencia; e.Notas = r.Notas;
    }

    private static GlobalNpcResponse Map(GlobalNpcEntity e) => new()
    {
        Id = e.Id, Name = e.Name, Source = e.Source, Tipo = e.Tipo, Ascendencia = e.Ascendencia, Level = e.Level,
        Fuerza = e.Fuerza, Velocidad = e.Velocidad, Intelecto = e.Intelecto,
        Voluntad = e.Voluntad, Discernimiento = e.Discernimiento, Presencia = e.Presencia,
        MaxHealth = e.MaxHealth, MaxConcentration = e.MaxConcentration, MaxInvestiture = e.MaxInvestiture,
        Agilidad = e.Agilidad, ArmasLigeras = e.ArmasLigeras, ArmasPesadas = e.ArmasPesadas,
        Atletismo = e.Atletismo, Hurto = e.Hurto, Sigilo = e.Sigilo,
        Deduccion = e.Deduccion, Disciplina = e.Disciplina, Intimidacion = e.Intimidacion,
        Manufactura = e.Manufactura, Medicina = e.Medicina, Conocimiento = e.Conocimiento,
        Engano = e.Engano, Liderazgo = e.Liderazgo, Percepcion = e.Percepcion,
        Perspicacia = e.Perspicacia, Persuasion = e.Persuasion, Supervivencia = e.Supervivencia,
        Talentos = e.Talentos, Apariencia = e.Apariencia, Notas = e.Notas,
        ImageUrl = e.ImageUrl, CreatedAt = e.CreatedAt, UpdatedAt = e.UpdatedAt,
        World = e.World, Era = e.Era,
    };
}
