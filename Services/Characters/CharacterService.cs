using Infrastructure.Data;
using Messages.Characters.In;
using Messages.Characters.Out;
using Messages.Database.Entities;
using Messages.Metas.Out;
using Microsoft.EntityFrameworkCore;
using Services.Worlds;

namespace Services.Characters;

public class CharacterService(CosmereContext db, IWorldRulesProvider reglas) : ICharacterService
{
    public async Task<List<CharacterResponse>> GetCharactersAsync(long campaignId, long userId)
    {
        await EnsureMemberAsync(campaignId, userId);
        var world = await GetWorldRulesAsync(campaignId);
        var isGm = await IsGmAsync(campaignId, userId);

        var query = db.Characters.Where(c => c.CampaignId == campaignId && !c.IsNpc);

        // Players only see their own character
        if (!isGm)
            query = query.Where(c => c.OwnerId == userId);

        var entities = await query.Include(c => c.Metas).ToListAsync();
        return entities.Select(c => MapToResponse(c, world, new ContextoJuego())).ToList();
    }

    public async Task<CharacterResponse> GetCharacterAsync(long characterId, long campaignId, long userId, ContextoJuego ctx)
    {
        await EnsureMemberAsync(campaignId, userId);
        var world = await GetWorldRulesAsync(campaignId);
        var isGm = await IsGmAsync(campaignId, userId);

        var character = await db.Characters
            .Include(c => c.Metas)
            .FirstOrDefaultAsync(c => c.Id == characterId && c.CampaignId == campaignId && !c.IsNpc)
            ?? throw new KeyNotFoundException("Character not found.");

        if (!isGm && character.OwnerId != userId)
            throw new UnauthorizedAccessException("You can only view your own character.");

        return MapToResponse(character, world, ctx);
    }

    public async Task<CharacterResponse> CreateCharacterAsync(long campaignId, CreateCharacterRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);
        var world = await GetWorldRulesAsync(campaignId);

        // Validate owner is a campaign member (if provided)
        if (request.OwnerId.HasValue)
        {
            var ownerIsMember = await db.CampaignMembers
                .AnyAsync(m => m.CampaignId == campaignId && m.UserId == request.OwnerId.Value);
            if (!ownerIsMember)
                throw new KeyNotFoundException("Assigned player is not a campaign member.");
        }

        world.ValidarIdentidad(new IdentidadPersonaje(
            request.CaminoHeroico, request.CaminoRadiante, request.CaminoMetal, request.CaminoInicial, request.Ascendencia,
            [], [], new Dictionary<string, decimal>()));

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
            CaminoMetal = request.CaminoMetal,
            CaminoInicial = request.CaminoInicial,
            IsNpc = false
        };

        db.Characters.Add(character);
        await db.SaveChangesAsync();
        return MapToResponse(character, world);
    }

    public async Task<CharacterResponse> UpdateCharacterAsync(long characterId, long campaignId, UpdateCharacterRequest request, long userId)
    {
        await EnsureMemberAsync(campaignId, userId);
        var world = await GetWorldRulesAsync(campaignId);

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
            world.RestringirCambiosNoGm(request, character);
        }

        // Identidad efectiva (lo que llega o, si es null, lo guardado), con la lista de poderes ya fusionada (§5.3).
        var poderes = CharacterJson.FusionarPoderes(CharacterJson.ParsePoderes(character.Poderes), request.Poderes);
        world.ValidarIdentidad(new IdentidadPersonaje(
            request.CaminoHeroico, request.CaminoRadiante,
            request.CaminoMetal ?? character.CaminoMetal, request.CaminoInicial ?? character.CaminoInicial,
            request.Ascendencia, poderes, request.Bendiciones ?? character.Bendiciones,
            CharacterJson.ParseRecursos(character.Recursos)));

        // Las reglas del mundo no tienen BD: la meta que enlaza un poder debe ser de este personaje.
        var metaIds = poderes.Where(p => p.MetaId is not null).Select(p => p.MetaId!.Value).Distinct().ToList();
        if (metaIds.Count > 0 &&
            await db.Metas.CountAsync(m => m.CharacterId == characterId && metaIds.Contains(m.Id)) != metaIds.Count)
            throw new ArgumentException("Invalid metaId in poderes: the meta must belong to this character.");

        ApplyUpdate(character, request);
        character.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return MapToResponse(character, world);
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
        var world = await GetWorldRulesAsync(campaignId);

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
        return MapToResponse(character, world);
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

    /// <summary>Reglas del mundo de la campaña; un mundo nulo o desconocido cae a Stormlight.</summary>
    private async Task<IWorldRules> GetWorldRulesAsync(long campaignId) =>
        reglas.Get(await db.Campaigns.AsNoTracking()
            .Where(c => c.Id == campaignId)
            .Select(c => c.World)
            .FirstOrDefaultAsync());

    private static void ApplyUpdate(CharacterEntity c, UpdateCharacterRequest r)
    {
        c.Name = r.Name; c.PlayerName = r.PlayerName; c.Level = r.Level; c.Experience = r.Experience;
        c.CaminoHeroico = r.CaminoHeroico; c.CaminoRadiante = r.CaminoRadiante; c.Ascendencia = r.Ascendencia;
        c.IdealesJurados = r.IdealesJurados;
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
        // Nacidos de la bruma: null = conservar lo guardado.
        if (r.CaminoMetal is not null) c.CaminoMetal = r.CaminoMetal;
        if (r.CaminoInicial is not null) c.CaminoInicial = r.CaminoInicial;
        if (r.Bendiciones is not null) c.Bendiciones = r.Bendiciones;
        if (r.Poderes is not null)
            c.Poderes = CharacterJson.SerializarPoderes(CharacterJson.FusionarPoderes(CharacterJson.ParsePoderes(c.Poderes), r.Poderes));
    }

    // ── Helpers de cálculo de reservas ───────────────────────────────────────

    // MaxConcentration y MaxInvestiture (bonus manuales) ya no se usan en el cálculo.
    // Los campos se mantienen en BD por compatibilidad pero están deprecados.

    // Los bonos de la forma activa de un cantor (Manual p. 33 del libro: «aumentos de características … de manera
    // temporal») se aplican a los atributos ANTES de derivar defensas, reservas y movimiento, y se muestran como
    // una línea «Forma X» en cada desglose.
    // Toda línea de bono de atributo (de cualquier origen) lleva EsBono = true: el cliente la reconoce sin leer el concepto.

    private static string ConceptoForma(string? forma) => $"Forma: {forma}";

    private static List<StatLinea> BuildDefensaLineas(string attr1, int valor1, string attr2, int valor2, int bonoForma, string? forma, string? etiquetaBono = null)
    {
        List<StatLinea> lineas =
        [
            new() { Concepto = "Base", Valor = 10 },
            new() { Concepto = attr1,  Valor = valor1 },
            new() { Concepto = attr2,  Valor = valor2 },
        ];
        if (bonoForma != 0) lineas.Add(new() { Concepto = etiquetaBono ?? ConceptoForma(forma), Valor = bonoForma, EsBono = true });
        return lineas;
    }

    private static List<StatLinea> BuildConcLineas(CharacterEntity c, BonosForma fb, string? forma, string? etiquetaBono = null)
    {
        List<StatLinea> lineas =
        [
            new() { Concepto = "Base",     Valor = 2 },
            new() { Concepto = "Voluntad", Valor = c.Voluntad },
        ];
        // La forma puede subir la Voluntad (→ +1 por punto) y/o dar concentración directa (forma diestra, nocturna: +2).
        var bono = fb.Voluntad + fb.Concentracion;
        if (bono != 0) lineas.Add(new() { Concepto = etiquetaBono ?? ConceptoForma(forma), Valor = bono, EsBono = true });
        return lineas;
    }

    private static List<StatLinea> BuildInvLineas(CharacterEntity c, BonosForma fb, string? forma, bool tieneInvestidura, string? etiquetaBono = null)
    {
        if (!tieneInvestidura)
            return [new() { Concepto = "Base", Valor = 0 }];

        var disEff = c.Discernimiento + fb.Discernimiento;
        var preEff = c.Presencia + fb.Presencia;
        var usaDis = disEff >= preEff;
        List<StatLinea> lineas =
        [
            new() { Concepto = "Base", Valor = 2 },
            new() { Concepto = usaDis ? "Discernimiento" : "Presencia", Valor = usaDis ? c.Discernimiento : c.Presencia },
        ];
        var bono = usaDis ? fb.Discernimiento : fb.Presencia;
        if (bono != 0) lineas.Add(new() { Concepto = etiquetaBono ?? ConceptoForma(forma), Valor = bono, EsBono = true });
        return lineas;
    }

    /// <summary>
    /// Salud máxima según tabla de progreso (cap. 1, p. 29).
    /// Nivel 1: 10 + FUE. Rangos 2–5: +5/nivel. Rango 6–10: +4/nivel + FUE.
    /// Rango 11–15: +3/nivel + FUE. Rango 16–20: +2/nivel + FUE. 21+: +1/nivel.
    /// Si la Fuerza cambia (también por una forma), la salud se recalcula con esa misma tabla (p. 54 del libro).
    /// </summary>
    private static List<StatLinea> BuildSaludLineas(CharacterEntity c, BonosForma fb, string? forma, string? etiquetaBono = null)
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

        List<StatLinea> lineas =
        [
            new() { Concepto = "Base",   Valor = flat },
            new() { Concepto = fueCount > 1 ? $"Fuerza ×{fueCount}" : "Fuerza", Valor = fueCount * fuerza },
        ];
        if (fb.Fuerza != 0) lineas.Add(new() { Concepto = etiquetaBono ?? ConceptoForma(forma), Valor = fueCount * fb.Fuerza, EsBono = true });
        return lineas;
    }

    /// <summary>
    /// Desvío efectivo. El desvío de una forma de cantor «no se acumula al de las armaduras … elige qué valor vas a
    /// usar» (Manual pp. 33–37 del libro), así que se toma el mayor y el otro queda como línea informativa.
    /// Con <paramref name="acumula"/> (un mundo cuyo bono de desvío sí se acumula a la armadura) se suman los dos.
    /// </summary>
    private static (List<StatLinea> Lineas, List<StatLinea> Situacional) BuildDesvioLineas(CharacterEntity c, BonosForma fb, string? forma, bool acumula = false, string? etiquetaBono = null)
    {
        var armadura   = new StatLinea { Concepto = string.IsNullOrEmpty(c.EquippedArmor) ? "Base" : $"Armadura: {c.EquippedArmor}", Valor = c.Desvio };
        if (fb.Desvio <= 0) return ([armadura], []);

        var formaLinea = new StatLinea { Concepto = etiquetaBono ?? ConceptoForma(forma), Valor = fb.Desvio, EsBono = true };
        if (acumula) return ([armadura, formaLinea], []);
        // La línea que no se usa queda como situacional, con la explicación de por qué no suma.
        var (gana, pierde) = fb.Desvio > c.Desvio ? (formaLinea, armadura) : (armadura, formaLinea);
        pierde.DescripcionCondicion = "No se acumula: se usa el mayor entre armadura y forma";
        return ([gana], pierde.Valor > 0 ? [pierde] : []);
    }

    private static List<string> ParseTalentos(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return [];
        try { return System.Text.Json.JsonSerializer.Deserialize<List<string>>(raw) ?? []; }
        catch { return []; }
    }

    internal static CharacterResponse MapToResponse(CharacterEntity c, IWorldRules world, ContextoJuego? ctx = null)
    {
        ctx ??= new ContextoJuego();
        var talentos = ParseTalentos(c.Talentos);
        // Talentos que el cliente concede solos (autoGranted) sin guardarlos y que las reglas del mundo deben contar.
        talentos = talentos.Union(world.TalentosImplicitos(c)).ToList();
        var poderes  = CharacterJson.ParsePoderes(c.Poderes);
        var recursos = CharacterJson.ParseRecursos(c.Recursos);
        var fb       = world.BonosAtributos(c, talentos, out var forma);
        var tieneInv = world.TieneInvestidura(c, talentos, poderes);
        var velEff   = c.Velocidad + fb.Velocidad;
        var desvio   = BuildDesvioLineas(c, fb, forma);

        return new CharacterResponse
        {
            Id = c.Id, CampaignId = c.CampaignId, OwnerId = c.OwnerId,
            Name = c.Name, PlayerName = c.PlayerName,
            Level = c.Level, Experience = c.Experience, CaminoHeroico = c.CaminoHeroico,
            CaminoRadiante = c.CaminoRadiante, Ascendencia = c.Ascendencia, IdealesJurados = c.IdealesJurados,
            Fuerza = c.Fuerza, Velocidad = c.Velocidad, Intelecto = c.Intelecto,
            Voluntad = c.Voluntad, Discernimiento = c.Discernimiento, Presencia = c.Presencia,
            MaxHealth = c.MaxHealth,
#pragma warning disable CS0618 // campos deprecados — mantenidos por compatibilidad
            MaxConcentration = c.MaxConcentration,
            MaxInvestiture   = c.MaxInvestiture,
#pragma warning restore CS0618
            Desvio = c.Desvio,
            MarcosInfusas = c.MarcosInfusas, MarcosOpacas = c.MarcosOpacas,

            // ── Stats calculadas ──────────────────────────────────────────────
            Concentracion = TalentosReglas.Calcular(
                StatAfectada.MaxConcentracion,
                BuildConcLineas(c, fb, forma),
                c, ctx, talentos, reglas: world.ReglasTalentos, tieneInvestidura: tieneInv),

            DefensaFisica = TalentosReglas.Calcular(
                StatAfectada.DefensaFisica,
                BuildDefensaLineas("Fuerza", c.Fuerza, "Velocidad", c.Velocidad, fb.Fuerza + fb.Velocidad, forma),
                c, ctx, talentos, reglas: world.ReglasTalentos, tieneInvestidura: tieneInv),

            DefensaCognitiva = TalentosReglas.Calcular(
                StatAfectada.DefensaCognitiva,
                BuildDefensaLineas("Intelecto", c.Intelecto, "Voluntad", c.Voluntad, fb.Intelecto + fb.Voluntad, forma),
                c, ctx, talentos, reglas: world.ReglasTalentos, tieneInvestidura: tieneInv),

            DefensaEspiritual = TalentosReglas.Calcular(
                StatAfectada.DefensaEspiritual,
                BuildDefensaLineas("Discernimiento", c.Discernimiento, "Presencia", c.Presencia, fb.Discernimiento + fb.Presencia, forma),
                c, ctx, talentos, reglas: world.ReglasTalentos, tieneInvestidura: tieneInv),

            Salud = TalentosReglas.Calcular(
                StatAfectada.MaxSalud,
                BuildSaludLineas(c, fb, forma),
                c, ctx, talentos, reglas: world.ReglasTalentos, tieneInvestidura: tieneInv),

            Investidura = TalentosReglas.Calcular(
                StatAfectada.MaxInvestidura,
                BuildInvLineas(c, fb, forma, tieneInv),
                c, ctx, talentos, reglas: world.ReglasTalentos, tieneInvestidura: tieneInv),

            // El movimiento depende de la Velocidad efectiva (con el bono de la forma, si lo hay).
            Movimiento = TalentosReglas.Calcular(
                StatAfectada.Movimiento,
                [new()
                {
                    Concepto = fb.Velocidad != 0 ? $"Velocidad ({c.Velocidad} + {fb.Velocidad} de {forma})" : $"Velocidad ({c.Velocidad})",
                    Valor = TalentosReglas.MovimientoBase(velEff),
                }],
                c, ctx, talentos, reglas: world.ReglasTalentos, tieneInvestidura: tieneInv, unidad: "m"),

            DesvioCalculado = TalentosReglas.Calcular(
                StatAfectada.Desvio,
                desvio.Lineas,
                c, ctx, talentos, reglas: world.ReglasTalentos, tieneInvestidura: tieneInv, situacionalBase: desvio.Situacional),

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

            // ── Nacidos de la bruma ───────────────────────────────────────────
            CaminoMetal = c.CaminoMetal, CaminoInicial = c.CaminoInicial,
            Poderes = poderes, Recursos = recursos, Bendiciones = c.Bendiciones,
        };
    }
}
