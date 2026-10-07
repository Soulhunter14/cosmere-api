using Infrastructure.Data;
using Messages.Characters.In;
using Messages.Characters.Out;
using Messages.Database.Entities;
using Messages.Metas.Out;
using Microsoft.EntityFrameworkCore;
using Services.Campaigns;
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

        // Camino inicial coherente con los caminos que llegan (§5.2, P6): se normaliza antes de validar, no se rechaza.
        request.CaminoMetal ??= string.Empty;
        request.CaminoInicial = NormalizarCaminoInicial(request.CaminoInicial, request.CaminoMetal, request.CaminoHeroico);

        world.ValidarIdentidad(new IdentidadPersonaje(
            request.CaminoHeroico, request.CaminoRadiante, request.CaminoMetal, request.CaminoInicial, request.Ascendencia,
            [], [], new Dictionary<string, decimal>()));
        ValidarNivel(request.Level);

        // Nivel 0 («El primer paso», ARTO006 PDF 5): los seis atributos empiezan en 1 y la aventura los sube o baja (0 a 3).
        int atributoInicial = request.Level == 0 ? 1 : 0;

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
            Fuerza = atributoInicial, Velocidad = atributoInicial, Intelecto = atributoInicial,
            Voluntad = atributoInicial, Discernimiento = atributoInicial, Presencia = atributoInicial,
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
            // Campaña iniciada: lo que se cerró al terminar la preparación ya no lo cambia un jugador (CierreCampana).
            var iniciadaEn = await db.Campaigns.Where(c => c.Id == campaignId).Select(c => c.IniciadaEn).FirstAsync();
            if (iniciadaEn is not null)
                CierreCampana.ConservarCampos(request, character);
        }

        // Camino inicial coherente con los caminos efectivos (§5.2, P6): si el GM borra el camino del que partía, pasa al otro
        // camino o a "" en lugar de dejar al personaje con un 400 permanente. El servidor no toca Talentos.
        request.CaminoInicial = NormalizarCaminoInicial(
            request.CaminoInicial ?? character.CaminoInicial, request.CaminoMetal ?? character.CaminoMetal, request.CaminoHeroico);

        // Identidad efectiva (lo que llega o, si es null, lo guardado), con la lista de poderes ya fusionada (§5.3).
        var poderes = CharacterJson.FusionarPoderes(CharacterJson.ParsePoderes(character.Poderes), request.Poderes);
        world.ValidarIdentidad(new IdentidadPersonaje(
            request.CaminoHeroico, request.CaminoRadiante,
            request.CaminoMetal ?? character.CaminoMetal, request.CaminoInicial ?? character.CaminoInicial,
            request.Ascendencia, poderes, request.Bendiciones ?? character.Bendiciones,
            CharacterJson.ParseRecursos(character.Recursos),
            request.Clavos ?? CharacterJson.ParseClavos(character.Clavos),
            request.Legado ?? character.Legado,
            request.LegadoRespuestas ?? character.LegadoRespuestas));
        ValidarNivel(request.Level);

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

    // ── Estado de mesa (T13) ─────────────────────────────────────────────────
    // Solo infraestructura: transacción con la fila bloqueada, permisos y respuesta. Las reglas (recortes, viales, Desprovisto,
    // inicio de escena) son del mundo: IWorldRules.AplicarAccionMesa (§5.2, §6.1).

    public Task<CharacterResponse> PatchRecursosAsync(long characterId, long campaignId, RecursosRequest request, long userId) =>
        AplicarAccionMesaAsync(characterId, campaignId, userId, esGm => new AccionMesa.PatchRecursos(request, esGm));

    public Task<CharacterResponse> BeberVialAsync(long characterId, long campaignId, BeberVialRequest request, long userId) =>
        AplicarAccionMesaAsync(characterId, campaignId, userId, _ => new AccionMesa.BeberVial(request.Metales ?? []));

    public Task<CharacterResponse> InicioEscenaAsync(long characterId, long campaignId, InicioEscenaRequest request, long userId) =>
        AplicarAccionMesaAsync(characterId, campaignId, userId, _ => new AccionMesa.InicioEscena(request.Sorprendido));

    private async Task<CharacterResponse> AplicarAccionMesaAsync(long characterId, long campaignId, long userId, Func<bool, AccionMesa> accion)
    {
        await EnsureMemberAsync(campaignId, userId);
        var world = await GetWorldRulesAsync(campaignId);
        var isGm = await IsGmAsync(campaignId, userId);

        // Las acciones leen la fila, reescriben las columnas JSON (Recursos, Poderes) enteras y devuelven el personaje: la fila se
        // bloquea hasta el commit para que dos peticiones simultáneas con claves distintas no se pisen. Mismos filtros y mismo
        // 404/403 que el PUT; nunca FirstAsync (sin filas lanzaría InvalidOperationException, que el middleware da como 409).
        await using var tx = await db.Database.BeginTransactionAsync();
        var character = await db.Characters
            .FromSqlInterpolated($"SELECT * FROM \"Characters\" WHERE \"Id\" = {characterId} AND \"CampaignId\" = {campaignId} AND NOT \"IsNpc\" FOR UPDATE")
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Character not found.");
        if (!isGm && character.OwnerId != userId)
            throw new UnauthorizedAccessException("You can only edit your own character.");

        var estado = MapToResponse(character, world);
        world.AplicarAccionMesa(character, accion(isGm), estado);
        character.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        // Como en GET: la respuesta lleva las metas (la fila se cargó sin Include).
        await db.Entry(character).Collection(c => c.Metas).LoadAsync();
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

    /// <summary>
    /// Nivel 0 es el de «El primer paso» (ARTO006): un personaje sin camino ni talentos que pasa a nivel 1 al final de la aventura.
    /// </summary>
    private static void ValidarNivel(int level)
    {
        if (level < 0) throw new ArgumentException($"Invalid Level: {level} (0 or more).");
    }

    /// <summary>
    /// <c>CaminoInicial</c> coherente con los caminos (§5.2, P6): <c>heroico</c> sin camino heroico pasa a <c>metal</c> si hay
    /// camino de nacido del metal (o a <c>""</c>); <c>metal</c> sin camino de nacido del metal pasa a <c>heroico</c> si hay
    /// camino heroico (o a <c>""</c>). Los valores fuera de dominio los rechaza después la validación del mundo.
    /// </summary>
    private static string NormalizarCaminoInicial(string? caminoInicial, string? caminoMetal, string? caminoHeroico)
    {
        var ci = caminoInicial ?? string.Empty;
        var cm = caminoMetal ?? string.Empty;
        var ch = caminoHeroico ?? string.Empty;
        if (ci == "heroico" && ch == "") ci = cm != "" ? "metal" : "";
        if (ci == "metal" && cm == "") ci = ch != "" ? "heroico" : "";
        return ci;
    }

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
        if (r.Clavos is not null) c.Clavos = CharacterJson.SerializarClavos(r.Clavos);
        if (r.Legado is not null) c.Legado = r.Legado;
        if (r.LegadoRespuestas is not null) c.LegadoRespuestas = r.LegadoRespuestas;
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
    /// Con el talento kandra Forma natural (<paramref name="talentos"/>) añade su +5 contra laceración como línea situacional.
    /// </summary>
    private static (List<StatLinea> Lineas, List<StatLinea> Situacional) BuildDesvioLineas(CharacterEntity c, BonosForma fb, string? forma, bool acumula = false, string? etiquetaBono = null, IEnumerable<string>? talentos = null)
    {
        var armadura   = new StatLinea { Concepto = string.IsNullOrEmpty(c.EquippedArmor) ? "Base" : $"Armadura: {c.EquippedArmor}", Valor = c.Desvio };
        // «Tu valor de desvío aumenta en 5 contra el daño por laceración» (Forma natural, también bajo Disfraz kandra;
        // L.35 / PDF 41): visible pero fuera del total.
        List<StatLinea> talento = talentos?.Contains("Forma natural") == true
            ? [new() { Concepto = "Forma natural", Valor = 5, DescripcionCondicion = "Contra daño por laceración" }]
            : [];
        if (fb.Desvio <= 0) return ([armadura], talento);

        var formaLinea = new StatLinea { Concepto = etiquetaBono ?? ConceptoForma(forma), Valor = fb.Desvio, EsBono = true };
        if (acumula) return ([armadura, formaLinea], talento);
        // La línea que no se usa queda como situacional, con la explicación de por qué no suma.
        var (gana, pierde) = fb.Desvio > c.Desvio ? (formaLinea, armadura) : (armadura, formaLinea);
        pierde.DescripcionCondicion = "No se acumula: se usa el mayor entre armadura y forma";
        return ([gana], pierde.Valor > 0 ? [pierde, .. talento] : talento);
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
        var etiqueta = world.EtiquetaBono(forma);
        var tieneInv = world.TieneInvestidura(c, talentos, poderes);
        var velEff   = c.Velocidad + fb.Velocidad;
        var desvio   = BuildDesvioLineas(c, fb, forma, world.DesvioBonoSeAcumula, etiqueta, talentos);

        // Cada desglose lo completa después el mundo con sus líneas propias (T49a: clavos hemalúrgicos en la Defensa espiritual).
        StatDesglose CalcularDesglose(StatAfectada stat, List<StatLinea> baseLineas, string? unidad = null, List<StatLinea>? situacionalBase = null)
        {
            var desglose = TalentosReglas.Calcular(stat, baseLineas, c, ctx, talentos,
                reglas: world.ReglasTalentos, tieneInvestidura: tieneInv, unidad: unidad, situacionalBase: situacionalBase);
            world.CompletarDesglose(c, stat, desglose);
            return desglose;
        }

        var response = new CharacterResponse
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
            Concentracion = CalcularDesglose(
                StatAfectada.MaxConcentracion,
                BuildConcLineas(c, fb, forma, etiqueta)),

            DefensaFisica = CalcularDesglose(
                StatAfectada.DefensaFisica,
                BuildDefensaLineas("Fuerza", c.Fuerza, "Velocidad", c.Velocidad, fb.Fuerza + fb.Velocidad, forma, etiqueta)),

            DefensaCognitiva = CalcularDesglose(
                StatAfectada.DefensaCognitiva,
                BuildDefensaLineas("Intelecto", c.Intelecto, "Voluntad", c.Voluntad, fb.Intelecto + fb.Voluntad, forma, etiqueta)),

            DefensaEspiritual = CalcularDesglose(
                StatAfectada.DefensaEspiritual,
                BuildDefensaLineas("Discernimiento", c.Discernimiento, "Presencia", c.Presencia, fb.Discernimiento + fb.Presencia, forma, etiqueta)),

            Salud = CalcularDesglose(
                StatAfectada.MaxSalud,
                BuildSaludLineas(c, fb, forma, etiqueta)),

            Investidura = CalcularDesglose(
                StatAfectada.MaxInvestidura,
                BuildInvLineas(c, fb, forma, tieneInv, etiqueta)),

            // El movimiento depende de la Velocidad efectiva (con el bono de la forma, si lo hay).
            Movimiento = CalcularDesglose(
                StatAfectada.Movimiento,
                [new()
                {
                    Concepto = fb.Velocidad != 0 ? $"Velocidad ({c.Velocidad} + {fb.Velocidad} de {forma})" : $"Velocidad ({c.Velocidad})",
                    Valor = TalentosReglas.MovimientoBase(velEff),
                }],
                unidad: "m"),

            DesvioCalculado = CalcularDesglose(
                StatAfectada.Desvio,
                desvio.Lineas,
                situacionalBase: desvio.Situacional),

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
            CaminoMetal = c.CaminoMetal, CaminoInicial = c.CaminoInicial, Legado = c.Legado,
            LegadoRespuestas = c.LegadoRespuestas,
            Poderes = poderes, Recursos = recursos, Bendiciones = c.Bendiciones,
            DerivadosSet = world.Derivar(c, talentos, poderes, fb),
            BonosAtributos = fb.ComoDiccionario(),
            Clavos = CharacterJson.ParseClavos(c.Clavos),
        };

        RecortarEstadoDeMesa(response);
        return response;
    }

    /// <summary>
    /// Recorte solo en la salida (no se persiste) del estado de mesa: la Investidura actual a [0, Investidura máxima] y las cargas
    /// de cada poder a [0, sus cargas máximas], por si el máximo bajó después de escribirlas (p. ej. al reducir Presencia en el
    /// <c>PUT</c>; §6.2).
    /// </summary>
    private static void RecortarEstadoDeMesa(CharacterResponse r)
    {
        if (r.Recursos.TryGetValue("investiduraActual", out var actual))
            r.Recursos["investiduraActual"] = Math.Clamp(actual, 0m, Math.Max(0m, (decimal)r.Investidura.Total));
        foreach (var p in r.Poderes)
        {
            if (r.DerivadosSet.TryGetValue($"poder.{p.Metal}.cargasMax", out var max))
                p.Cargas = Math.Clamp(p.Cargas, 0, Math.Max(0, (int)max.Total));
        }
    }
}
