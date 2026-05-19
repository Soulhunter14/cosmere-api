using Infrastructure.Data;
using Messages.Characters.In;
using Messages.Characters.Out;
using Messages.Database.Entities;
using Messages.Metas.Out;
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

        var entities = await query.Include(c => c.Metas).ToListAsync();
        return entities.Select(c => MapToResponse(c, new ContextoJuego())).ToList();
    }

    public async Task<CharacterResponse> GetCharacterAsync(long characterId, long campaignId, long userId, ContextoJuego ctx)
    {
        await EnsureMemberAsync(campaignId, userId);
        var isGm = await IsGmAsync(campaignId, userId);

        var character = await db.Characters
            .Include(c => c.Metas)
            .FirstOrDefaultAsync(c => c.Id == characterId && c.CampaignId == campaignId && !c.IsNpc)
            ?? throw new KeyNotFoundException("Character not found.");

        if (!isGm && character.OwnerId != userId)
            throw new UnauthorizedAccessException("You can only view your own character.");

        return MapToResponse(character, ctx);
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

        if (!isGm)
        {
            request.Name = character.Name;
            request.CaminoHeroico = character.CaminoHeroico;
            request.CaminoRadiante = character.CaminoRadiante;
        }

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
        c.MaxHealth = r.MaxHealth;
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
        c.HabilidadPersonalizada4 = r.HabilidadPersonalizada4; c.HabilidadPersonalizada4Valor = r.HabilidadPersonalizada4Valor; c.HabilidadPersonalizada4Atributo = r.HabilidadPersonalizada4Atributo;
        c.HabilidadPersonalizada5 = r.HabilidadPersonalizada5; c.HabilidadPersonalizada5Valor = r.HabilidadPersonalizada5Valor; c.HabilidadPersonalizada5Atributo = r.HabilidadPersonalizada5Atributo;
        c.HabilidadPersonalizada6 = r.HabilidadPersonalizada6; c.HabilidadPersonalizada6Valor = r.HabilidadPersonalizada6Valor; c.HabilidadPersonalizada6Atributo = r.HabilidadPersonalizada6Atributo;
        c.Proposito = r.Proposito; c.Obstaculo = r.Obstaculo;
        c.Talentos = r.Talentos; c.Apariencia = r.Apariencia; c.Notas = r.Notas; c.Conexiones = r.Conexiones;
        c.Weapons = r.Weapons; c.Armor = r.Armor; c.Spells = r.Spells; c.Equipment = r.Equipment;
        c.EquippedArmor = r.Armor.Contains(r.EquippedArmor) ? r.EquippedArmor : string.Empty;
    }

    // ── Helpers de cálculo de reservas ───────────────────────────────────────

    private static List<StatLinea> BuildConcLineas(CharacterEntity c)
    {
        var lineas = new List<StatLinea>
        {
            new() { Concepto = "Base",     Valor = 2 },
            new() { Concepto = "Voluntad", Valor = c.Voluntad },
        };
        if (c.MaxConcentration > 0)
            lineas.Add(new() { Concepto = "Bonus", Valor = c.MaxConcentration });
        return lineas;
    }

    private static List<StatLinea> BuildInvLineas(CharacterEntity c)
    {
        // Personajes sin camino Radiante no tienen Investidura
        if (string.IsNullOrEmpty(c.CaminoRadiante))
            return [new() { Concepto = "Base", Valor = 0 }];

        var atributo  = c.Discernimiento >= c.Presencia ? "Discernimiento" : "Presencia";
        var valorAtrib = Math.Max(c.Discernimiento, c.Presencia);
        var lineas = new List<StatLinea>
        {
            new() { Concepto = "Base",    Valor = 2 },
            new() { Concepto = atributo,  Valor = valorAtrib },
        };
        if (c.MaxInvestiture > 0)
            lineas.Add(new() { Concepto = "Bonus", Valor = c.MaxInvestiture });
        return lineas;
    }

    /// <summary>
    /// Salud máxima según tabla de progreso (cap. 1, p. 29).
    /// Nivel 1: 10 + FUE. Rangos 2–5: +5/nivel. Rango 6–10: +4/nivel + FUE.
    /// Rango 11–15: +3/nivel + FUE. Rango 16–20: +2/nivel + FUE. 21+: +1/nivel.
    /// </summary>
    private static List<StatLinea> BuildSaludLineas(CharacterEntity c)
    {
        int level  = c.Level;
        int fuerza = c.Fuerza;

        int flat     = 10;
        int fueCount = 1;

        if (level >= 2)  flat += (Math.Min(level, 5)  - 1) * 5;
        if (level >= 6)  { flat += (Math.Min(level, 10) - 5)  * 4; fueCount++; }
        if (level >= 11) { flat += (Math.Min(level, 15) - 10) * 3; fueCount++; }
        if (level >= 16) { flat += (Math.Min(level, 20) - 15) * 2; fueCount++; }
        if (level >= 21) flat += level - 20;

        return
        [
            new() { Concepto = "Base",   Valor = flat },
            new() { Concepto = fueCount > 1 ? $"Fuerza ×{fueCount}" : "Fuerza", Valor = fueCount * fuerza },
        ];
    }

    private static List<string> ParseTalentos(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return [];
        try { return System.Text.Json.JsonSerializer.Deserialize<List<string>>(raw) ?? []; }
        catch { return []; }
    }

    internal static CharacterResponse MapToResponse(CharacterEntity c, ContextoJuego? ctx = null)
    {
        ctx ??= new ContextoJuego();
        var talentos = ParseTalentos(c.Talentos);

        return new CharacterResponse
        {
            Id = c.Id, CampaignId = c.CampaignId, OwnerId = c.OwnerId,
            Name = c.Name, PlayerName = c.PlayerName,
            Level = c.Level, Experience = c.Experience, CaminoHeroico = c.CaminoHeroico,
            CaminoRadiante = c.CaminoRadiante, Ascendencia = c.Ascendencia,
            Fuerza = c.Fuerza, Velocidad = c.Velocidad, Intelecto = c.Intelecto,
            Voluntad = c.Voluntad, Discernimiento = c.Discernimiento, Presencia = c.Presencia,
            MaxHealth = c.MaxHealth, MaxConcentration = c.MaxConcentration,
            MaxInvestiture = c.MaxInvestiture, Desvio = c.Desvio,
            MarcosInfusas = c.MarcosInfusas, MarcosOpacas = c.MarcosOpacas,

            // ── Stats calculadas ──────────────────────────────────────────────
            Concentracion = TalentosReglas.Calcular(
                StatAfectada.MaxConcentracion,
                BuildConcLineas(c),
                c, ctx, talentos),

            DefensaFisica = TalentosReglas.Calcular(
                StatAfectada.DefensaFisica,
                [new() { Concepto = "Base", Valor = 10 }, new() { Concepto = "Fuerza", Valor = c.Fuerza }, new() { Concepto = "Velocidad", Valor = c.Velocidad }],
                c, ctx, talentos),

            DefensaCognitiva = TalentosReglas.Calcular(
                StatAfectada.DefensaCognitiva,
                [new() { Concepto = "Base", Valor = 10 }, new() { Concepto = "Intelecto", Valor = c.Intelecto }, new() { Concepto = "Voluntad", Valor = c.Voluntad }],
                c, ctx, talentos),

            DefensaEspiritual = TalentosReglas.Calcular(
                StatAfectada.DefensaEspiritual,
                [new() { Concepto = "Base", Valor = 10 }, new() { Concepto = "Discernimiento", Valor = c.Discernimiento }, new() { Concepto = "Presencia", Valor = c.Presencia }],
                c, ctx, talentos),

            Salud = TalentosReglas.Calcular(
                StatAfectada.MaxSalud,
                BuildSaludLineas(c),
                c, ctx, talentos),

            Investidura = TalentosReglas.Calcular(
                StatAfectada.MaxInvestidura,
                BuildInvLineas(c),
                c, ctx, talentos),

            Movimiento = TalentosReglas.Calcular(
                StatAfectada.Movimiento,
                [new() { Concepto = $"Velocidad ({c.Velocidad})", Valor = TalentosReglas.MovimientoBase(c.Velocidad) }],
                c, ctx, talentos, unidad: "m"),

            // ── Resto de campos ───────────────────────────────────────────────
            Agilidad = c.Agilidad, ArmasLigeras = c.ArmasLigeras, ArmasPesadas = c.ArmasPesadas,
            Atletismo = c.Atletismo, Hurto = c.Hurto, Sigilo = c.Sigilo, Deduccion = c.Deduccion,
            Disciplina = c.Disciplina, Intimidacion = c.Intimidacion, Manufactura = c.Manufactura,
            Medicina = c.Medicina, Conocimiento = c.Conocimiento, Engano = c.Engano,
            Liderazgo = c.Liderazgo, Percepcion = c.Percepcion, Perspicacia = c.Perspicacia,
            Persuasion = c.Persuasion, Supervivencia = c.Supervivencia,
            HabilidadPersonalizada1 = c.HabilidadPersonalizada1, HabilidadPersonalizada1Valor = c.HabilidadPersonalizada1Valor, HabilidadPersonalizada1Atributo = c.HabilidadPersonalizada1Atributo,
            HabilidadPersonalizada2 = c.HabilidadPersonalizada2, HabilidadPersonalizada2Valor = c.HabilidadPersonalizada2Valor, HabilidadPersonalizada2Atributo = c.HabilidadPersonalizada2Atributo,
            HabilidadPersonalizada3 = c.HabilidadPersonalizada3, HabilidadPersonalizada3Valor = c.HabilidadPersonalizada3Valor, HabilidadPersonalizada3Atributo = c.HabilidadPersonalizada3Atributo,
            HabilidadPersonalizada4 = c.HabilidadPersonalizada4, HabilidadPersonalizada4Valor = c.HabilidadPersonalizada4Valor, HabilidadPersonalizada4Atributo = c.HabilidadPersonalizada4Atributo,
            HabilidadPersonalizada5 = c.HabilidadPersonalizada5, HabilidadPersonalizada5Valor = c.HabilidadPersonalizada5Valor, HabilidadPersonalizada5Atributo = c.HabilidadPersonalizada5Atributo,
            HabilidadPersonalizada6 = c.HabilidadPersonalizada6, HabilidadPersonalizada6Valor = c.HabilidadPersonalizada6Valor, HabilidadPersonalizada6Atributo = c.HabilidadPersonalizada6Atributo,
            Proposito = c.Proposito, Obstaculo = c.Obstaculo, Talentos = c.Talentos,
            Metas = c.Metas.Select(m => new MetaResponse
            {
                Id = m.Id, CharacterId = m.CharacterId, Titulo = m.Titulo,
                Descripcion = m.Descripcion, Hitos = m.Hitos, Estado = m.Estado,
                TipoConclusion = m.TipoConclusion, NotasConclusion = m.NotasConclusion,
                CreatedAt = m.CreatedAt,
            }).ToList(),
            Apariencia = c.Apariencia, Notas = c.Notas, Conexiones = c.Conexiones,
            Weapons = c.Weapons, Armor = c.Armor, Spells = c.Spells, Equipment = c.Equipment,
            EquippedArmor = c.EquippedArmor,
            CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt,
        };
    }
}
