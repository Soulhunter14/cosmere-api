using Infrastructure.Data;
using Messages.Characters.In;
using Messages.Characters.Out;
using Messages.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Services.Characters;

public class CharacterService(CosmereContext db) : ICharacterService
{
    private static readonly HashSet<string> ValidCaminosHeroicos =
    [
        "agente", "cazador", "enviado", "erudito", "guerrero", "lider"
    ];

    private static readonly HashSet<string> ValidCaminosRadiantes =
    [
        "windrunners", "skybreakers", "dustbringers", "edgedancers", "truthwatchers",
        "lightweavers", "elsecallers", "willshapers", "stonewards", "bondsmiths"
    ];

    private static readonly HashSet<string> ValidAscendencias = ["Humano", "Oyente"];

    private static void ValidateCaminos(string caminoHeroico, string caminoRadiante, string ascendencia)
    {
        if (!string.IsNullOrEmpty(caminoHeroico) && !ValidCaminosHeroicos.Contains(caminoHeroico))
            throw new ArgumentException($"Invalid CaminoHeroico: '{caminoHeroico}'.");
        if (!string.IsNullOrEmpty(caminoRadiante) && !ValidCaminosRadiantes.Contains(caminoRadiante))
            throw new ArgumentException($"Invalid CaminoRadiante: '{caminoRadiante}'.");
        if (!string.IsNullOrEmpty(ascendencia) && !ValidAscendencias.Contains(ascendencia))
            throw new ArgumentException($"Invalid Ascendencia: '{ascendencia}'.");
    }

    public async Task<List<CharacterResponse>> GetCharactersAsync(long campaignId, long userId)
    {
        await EnsureMemberAsync(campaignId, userId);
        var isGm = await IsGmAsync(campaignId, userId);

        var query = db.Characters.Where(c => c.CampaignId == campaignId && !c.IsNpc);

        // Players only see their own character
        if (!isGm)
            query = query.Where(c => c.OwnerId == userId);

        return await query.Select(c => MapToResponse(c)).ToListAsync();
    }

    public async Task<CharacterResponse> GetCharacterAsync(long characterId, long campaignId, long userId)
    {
        await EnsureMemberAsync(campaignId, userId);
        var isGm = await IsGmAsync(campaignId, userId);

        var character = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == characterId && c.CampaignId == campaignId && !c.IsNpc)
            ?? throw new KeyNotFoundException("Character not found.");

        if (!isGm && character.OwnerId != userId)
            throw new UnauthorizedAccessException("You can only view your own character.");

        return MapToResponse(character);
    }

    public async Task<CharacterResponse> CreateCharacterAsync(long campaignId, CreateCharacterRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        // Validate owner is a campaign member (if provided)
        if (request.OwnerId.HasValue)
        {
            var ownerIsMember = await db.CampaignMembers
                .AnyAsync(m => m.CampaignId == campaignId && m.UserId == request.OwnerId.Value);
            if (!ownerIsMember)
                throw new KeyNotFoundException("Assigned player is not a campaign member.");
        }

        ValidateCaminos(request.CaminoHeroico, request.CaminoRadiante, request.Ascendencia);

        var character = new CharacterEntity
        {
            CampaignId = campaignId,
            OwnerId = request.OwnerId,
            Name = request.Name,
            PlayerName = request.PlayerName,
            Level = request.Level,
            Ascendencia = request.Ascendencia,
            CaminoHeroico = request.CaminoHeroico,
            CaminoRadiante = request.CaminoRadiante,
            IsNpc = false
        };

        db.Characters.Add(character);
        await db.SaveChangesAsync();
        return MapToResponse(character);
    }

    public async Task<CharacterResponse> UpdateCharacterAsync(long characterId, long campaignId, UpdateCharacterRequest request, long userId)
    {
        await EnsureMemberAsync(campaignId, userId);

        var character = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == characterId && c.CampaignId == campaignId && !c.IsNpc)
            ?? throw new KeyNotFoundException("Character not found.");

        var isGm = await IsGmAsync(campaignId, userId);
        if (!isGm && character.OwnerId != userId)
            throw new UnauthorizedAccessException("You can only edit your own character.");

        ValidateCaminos(request.CaminoHeroico, request.CaminoRadiante, request.Ascendencia);
        ApplyUpdate(character, request);
        character.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return MapToResponse(character);
    }

    public async Task DeleteCharacterAsync(long characterId, long campaignId, long userId)
    {
        await EnsureGmAsync(campaignId, userId);
        var character = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == characterId && c.CampaignId == campaignId && !c.IsNpc)
            ?? throw new KeyNotFoundException("Character not found.");
        db.Characters.Remove(character);
        await db.SaveChangesAsync();
    }

    public async Task<CharacterResponse> AssignCharacterAsync(long characterId, long campaignId, long? ownerId, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        var character = await db.Characters
            .FirstOrDefaultAsync(c => c.Id == characterId && c.CampaignId == campaignId && !c.IsNpc)
            ?? throw new KeyNotFoundException("Character not found.");

        if (ownerId.HasValue)
        {
            var ownerIsMember = await db.CampaignMembers
                .AnyAsync(m => m.CampaignId == campaignId && m.UserId == ownerId.Value);
            if (!ownerIsMember)
                throw new KeyNotFoundException("Assigned player is not a campaign member.");
        }

        character.OwnerId = ownerId;
        character.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return MapToResponse(character);
    }

    private async Task<bool> IsGmAsync(long campaignId, long userId)
        => await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId && m.Role == "gm");

    private async Task EnsureMemberAsync(long campaignId, long userId)
    {
        var isMember = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId);
        if (!isMember) throw new KeyNotFoundException("Campaign not found.");
    }

    private async Task EnsureGmAsync(long campaignId, long userId)
    {
        if (!await IsGmAsync(campaignId, userId))
            throw new UnauthorizedAccessException("Only the GM can perform this action.");
    }

    private static void ApplyUpdate(CharacterEntity c, UpdateCharacterRequest r)
    {
        c.Name = r.Name; c.PlayerName = r.PlayerName; c.Level = r.Level; c.Experience = r.Experience;
        c.CaminoHeroico = r.CaminoHeroico; c.CaminoRadiante = r.CaminoRadiante; c.Ascendencia = r.Ascendencia;
        c.Fuerza = r.Fuerza; c.Velocidad = r.Velocidad; c.Intelecto = r.Intelecto;
        c.Voluntad = r.Voluntad; c.Discernimiento = r.Discernimiento; c.Presencia = r.Presencia;
        c.Health = r.Health; c.MaxHealth = r.MaxHealth;
        c.MaxConcentration = r.MaxConcentration;
        c.MaxInvestiture = r.MaxInvestiture; c.Desvio = r.Desvio;
        c.MarcosInfusas = r.MarcosInfusas; c.MarcosOpacas = r.MarcosOpacas;
        c.Agilidad = r.Agilidad; c.ArmasLigeras = r.ArmasLigeras; c.ArmasPesadas = r.ArmasPesadas;
        c.Atletismo = r.Atletismo; c.Hurto = r.Hurto; c.Sigilo = r.Sigilo; c.Deduccion = r.Deduccion;
        c.Disciplina = r.Disciplina; c.Intimidacion = r.Intimidacion; c.Manufactura = r.Manufactura;
        c.Medicina = r.Medicina; c.Conocimiento = r.Conocimiento; c.Engano = r.Engano;
        c.Liderazgo = r.Liderazgo; c.Percepcion = r.Percepcion; c.Perspicacia = r.Perspicacia;
        c.Persuasion = r.Persuasion; c.Supervivencia = r.Supervivencia;
        c.HabilidadPersonalizada1 = r.HabilidadPersonalizada1; c.HabilidadPersonalizada1Valor = r.HabilidadPersonalizada1Valor; c.HabilidadPersonalizada1Atributo = r.HabilidadPersonalizada1Atributo;
        c.HabilidadPersonalizada2 = r.HabilidadPersonalizada2; c.HabilidadPersonalizada2Valor = r.HabilidadPersonalizada2Valor; c.HabilidadPersonalizada2Atributo = r.HabilidadPersonalizada2Atributo;
        c.HabilidadPersonalizada3 = r.HabilidadPersonalizada3; c.HabilidadPersonalizada3Valor = r.HabilidadPersonalizada3Valor; c.HabilidadPersonalizada3Atributo = r.HabilidadPersonalizada3Atributo;
        c.Proposito = r.Proposito; c.Obstaculo = r.Obstaculo; c.Metas = r.Metas;
        c.Talentos = r.Talentos; c.Apariencia = r.Apariencia; c.Notas = r.Notas; c.Conexiones = r.Conexiones;
        c.Weapons = r.Weapons; c.Armor = r.Armor; c.Spells = r.Spells; c.Equipment = r.Equipment;
        c.EquippedArmor = r.Armor.Contains(r.EquippedArmor) ? r.EquippedArmor : string.Empty;
    }

    internal static CharacterResponse MapToResponse(CharacterEntity c) => new()
    {
        Id = c.Id, CampaignId = c.CampaignId, OwnerId = c.OwnerId,
        Name = c.Name, PlayerName = c.PlayerName,
        Level = c.Level, Experience = c.Experience, CaminoHeroico = c.CaminoHeroico,
        CaminoRadiante = c.CaminoRadiante, Ascendencia = c.Ascendencia,
        Fuerza = c.Fuerza, Velocidad = c.Velocidad, Intelecto = c.Intelecto,
        Voluntad = c.Voluntad, Discernimiento = c.Discernimiento, Presencia = c.Presencia,
        Health = c.Health, MaxHealth = c.MaxHealth,
        MaxConcentration = c.MaxConcentration,
        MaxInvestiture = c.MaxInvestiture, Desvio = c.Desvio,
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
        EquippedArmor = c.EquippedArmor,
        CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
    };
}
