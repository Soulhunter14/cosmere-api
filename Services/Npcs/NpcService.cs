using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.Npcs.In;
using Messages.Npcs.Out;
using Microsoft.EntityFrameworkCore;

namespace Services.Npcs;

public class NpcService(CosmereContext db) : INpcService
{
    public async Task<List<NpcResponse>> GetNpcsAsync(long campaignId, long userId)
    {
        var isGm = await IsGmAsync(campaignId, userId);
        if (!isGm)
        {
            var isMember = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId);
            if (!isMember) throw new KeyNotFoundException("Campaign not found.");
        }

        var query = db.Characters.Where(c => c.CampaignId == campaignId && c.IsNpc);
        if (!isGm) query = query.Where(c => c.IsVisibleToPlayers);

        return await query.Select(c => MapToResponse(c)).ToListAsync();
    }

    public async Task<NpcResponse> GetNpcAsync(long npcId, long campaignId, long userId)
    {
        var isGm = await IsGmAsync(campaignId, userId);
        var npc = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == npcId && c.CampaignId == campaignId && c.IsNpc)
            ?? throw new KeyNotFoundException("NPC not found.");
        if (!isGm && !npc.IsVisibleToPlayers) throw new KeyNotFoundException("NPC not found.");
        return MapToResponse(npc);
    }

    public async Task<NpcResponse> CreateNpcAsync(long campaignId, CreateNpcRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);
        var npc = new CharacterEntity
        {
            CampaignId = campaignId,
            IsNpc = true,
            Name = request.Name,
            IsVisibleToPlayers = request.IsVisibleToPlayers,
            Apariencia = request.Apariencia,
            Notas = request.Notas,
            CaminoHeroico = request.CaminoHeroico,
            CaminoRadiante = request.CaminoRadiante,
            Ascendencia = request.Ascendencia,
            Level = request.Level,
            Fuerza = request.Fuerza, Velocidad = request.Velocidad, Intelecto = request.Intelecto,
            Voluntad = request.Voluntad, Discernimiento = request.Discernimiento, Presencia = request.Presencia,
            Health = request.Health, MaxHealth = request.MaxHealth, Desvio = request.Desvio,
            Weapons = request.Weapons, Armor = request.Armor
        };
        db.Characters.Add(npc);
        await db.SaveChangesAsync();
        return MapToResponse(npc);
    }

    public async Task<NpcResponse> UpdateNpcAsync(long npcId, long campaignId, UpdateNpcRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);
        var npc = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == npcId && c.CampaignId == campaignId && c.IsNpc)
            ?? throw new KeyNotFoundException("NPC not found.");

        npc.Name = request.Name; npc.IsVisibleToPlayers = request.IsVisibleToPlayers;
        npc.Apariencia = request.Apariencia; npc.Notas = request.Notas;
        npc.CaminoHeroico = request.CaminoHeroico; npc.CaminoRadiante = request.CaminoRadiante;
        npc.Ascendencia = request.Ascendencia; npc.Level = request.Level;
        npc.Fuerza = request.Fuerza; npc.Velocidad = request.Velocidad; npc.Intelecto = request.Intelecto;
        npc.Voluntad = request.Voluntad; npc.Discernimiento = request.Discernimiento; npc.Presencia = request.Presencia;
        npc.Health = request.Health; npc.MaxHealth = request.MaxHealth;
        npc.Concentration = request.Concentration; npc.MaxConcentration = request.MaxConcentration;
        npc.Investiture = request.Investiture; npc.MaxInvestiture = request.MaxInvestiture; npc.Desvio = request.Desvio;
        npc.MarcosInfusas = request.MarcosInfusas; npc.MarcosOpacas = request.MarcosOpacas;
        npc.Agilidad = request.Agilidad; npc.ArmasLigeras = request.ArmasLigeras; npc.ArmasPesadas = request.ArmasPesadas;
        npc.Atletismo = request.Atletismo; npc.Hurto = request.Hurto; npc.Sigilo = request.Sigilo;
        npc.Deduccion = request.Deduccion; npc.Disciplina = request.Disciplina; npc.Intimidacion = request.Intimidacion;
        npc.Manufactura = request.Manufactura; npc.Medicina = request.Medicina; npc.Conocimiento = request.Conocimiento;
        npc.Engano = request.Engano; npc.Liderazgo = request.Liderazgo; npc.Percepcion = request.Percepcion;
        npc.Perspicacia = request.Perspicacia; npc.Persuasion = request.Persuasion; npc.Supervivencia = request.Supervivencia;
        npc.HabilidadPersonalizada1 = request.HabilidadPersonalizada1; npc.HabilidadPersonalizada1Valor = request.HabilidadPersonalizada1Valor; npc.HabilidadPersonalizada1Atributo = request.HabilidadPersonalizada1Atributo;
        npc.HabilidadPersonalizada2 = request.HabilidadPersonalizada2; npc.HabilidadPersonalizada2Valor = request.HabilidadPersonalizada2Valor; npc.HabilidadPersonalizada2Atributo = request.HabilidadPersonalizada2Atributo;
        npc.HabilidadPersonalizada3 = request.HabilidadPersonalizada3; npc.HabilidadPersonalizada3Valor = request.HabilidadPersonalizada3Valor; npc.HabilidadPersonalizada3Atributo = request.HabilidadPersonalizada3Atributo;
        npc.Proposito = request.Proposito; npc.Obstaculo = request.Obstaculo; npc.Metas = request.Metas;
        npc.Talentos = request.Talentos; npc.Conexiones = request.Conexiones;
        npc.Weapons = request.Weapons; npc.Armor = request.Armor;
        npc.Spells = request.Spells; npc.Equipment = request.Equipment;
        npc.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return MapToResponse(npc);
    }

    public async Task<NpcResponse> CloneNpcAsync(long npcId, long campaignId, long userId)
    {
        await EnsureGmAsync(campaignId, userId);
        var source = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == npcId && c.CampaignId == campaignId && c.IsNpc)
            ?? throw new KeyNotFoundException("NPC not found.");

        var clone = new CharacterEntity
        {
            CampaignId = campaignId, IsNpc = true,
            Name = $"{source.Name} (copia)", IsVisibleToPlayers = source.IsVisibleToPlayers,
            Level = source.Level, Fuerza = source.Fuerza, Velocidad = source.Velocidad,
            Intelecto = source.Intelecto, Voluntad = source.Voluntad,
            Discernimiento = source.Discernimiento, Presencia = source.Presencia,
            Health = source.Health, MaxHealth = source.MaxHealth,
            Desvio = source.Desvio, Apariencia = source.Apariencia,
            Notas = source.Notas, CaminoHeroico = source.CaminoHeroico,
            CaminoRadiante = source.CaminoRadiante, Ascendencia = source.Ascendencia,
            Weapons = new List<string>(source.Weapons), Armor = new List<string>(source.Armor),
            Spells = new List<string>(source.Spells), Equipment = new List<string>(source.Equipment)
        };
        db.Characters.Add(clone);
        await db.SaveChangesAsync();
        return MapToResponse(clone);
    }

    public async Task ToggleVisibilityAsync(long npcId, long campaignId, ToggleNpcVisibilityRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);
        var npc = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == npcId && c.CampaignId == campaignId && c.IsNpc)
            ?? throw new KeyNotFoundException("NPC not found.");
        npc.IsVisibleToPlayers = request.IsVisibleToPlayers;
        await db.SaveChangesAsync();
    }

    public async Task DeleteNpcAsync(long npcId, long campaignId, long userId)
    {
        await EnsureGmAsync(campaignId, userId);
        var npc = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == npcId && c.CampaignId == campaignId && c.IsNpc)
            ?? throw new KeyNotFoundException("NPC not found.");
        db.Characters.Remove(npc);
        await db.SaveChangesAsync();
    }

    private async Task<bool> IsGmAsync(long campaignId, long userId) =>
        await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId && m.Role == "gm");

    private async Task EnsureGmAsync(long campaignId, long userId)
    {
        if (!await IsGmAsync(campaignId, userId))
            throw new UnauthorizedAccessException("Only the GM can perform this action.");
    }

    private static NpcResponse MapToResponse(CharacterEntity c) => new()
    {
        Id = c.Id, CampaignId = c.CampaignId, Name = c.Name, IsVisibleToPlayers = c.IsVisibleToPlayers,
        Level = c.Level, Experience = c.Experience, CaminoHeroico = c.CaminoHeroico,
        CaminoRadiante = c.CaminoRadiante, Ascendencia = c.Ascendencia,
        Fuerza = c.Fuerza, Velocidad = c.Velocidad, Intelecto = c.Intelecto,
        Voluntad = c.Voluntad, Discernimiento = c.Discernimiento, Presencia = c.Presencia,
        Health = c.Health, MaxHealth = c.MaxHealth,
        Concentration = c.Concentration, MaxConcentration = c.MaxConcentration,
        Investiture = c.Investiture, MaxInvestiture = c.MaxInvestiture, Desvio = c.Desvio,
        MarcosInfusas = c.MarcosInfusas, MarcosOpacas = c.MarcosOpacas,
        Agilidad = c.Agilidad, ArmasLigeras = c.ArmasLigeras, ArmasPesadas = c.ArmasPesadas,
        Atletismo = c.Atletismo, Hurto = c.Hurto, Sigilo = c.Sigilo, Deduccion = c.Deduccion,
        Disciplina = c.Disciplina, Intimidacion = c.Intimidacion, Manufactura = c.Manufactura,
        Medicina = c.Medicina, Conocimiento = c.Conocimiento, Engano = c.Engano,
        Liderazgo = c.Liderazgo, Percepcion = c.Percepcion, Perspicacia = c.Perspicacia,
        Persuasion = c.Persuasion, Supervivencia = c.Supervivencia,
        HabilidadPersonalizada1 = c.HabilidadPersonalizada1, HabilidadPersonalizada1Valor = c.HabilidadPersonalizada1Valor, HabilidadPersonalizada1Atributo = c.HabilidadPersonalizada1Atributo,
        HabilidadPersonalizada2 = c.HabilidadPersonalizada2, HabilidadPersonalizada2Valor = c.HabilidadPersonalizada2Valor, HabilidadPersonalizada2Atributo = c.HabilidadPersonalizada2Atributo,
        HabilidadPersonalizada3 = c.HabilidadPersonalizada3, HabilidadPersonalizada3Valor = c.HabilidadPersonalizada3Valor, HabilidadPersonalizada3Atributo = c.HabilidadPersonalizada3Atributo,
        Proposito = c.Proposito, Obstaculo = c.Obstaculo, Metas = c.Metas, Talentos = c.Talentos,
        Apariencia = c.Apariencia, Notas = c.Notas, Conexiones = c.Conexiones,
        Weapons = c.Weapons, Armor = c.Armor, Spells = c.Spells, Equipment = c.Equipment,
        CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
    };
}
