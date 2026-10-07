using System.Text.Json;
using Messages.Characters;
using Messages.Characters.In;
using Messages.Characters.Out;
using Messages.Database.Entities;
using Messages.Worlds;
using Services.Characters;

namespace Services.Worlds;

/// <summary>
/// Reglas del mundo «Nacidos de la bruma» (Scadrial): validación de la identidad (§5.3), bloqueo de los cambios de un jugador no
/// GM (Q6, Q16), condición de Investidura, Resistencia koloss, Bendiciones kandra, talentos de atributo y clavos hemalúrgicos de
/// atributo como bonos, derivados de las artes metálicas (<c>DerivadosSet</c>), acciones de mesa (<c>PATCH recursos</c>, Beber vial
/// e inicio de escena: T13) y hemalurgia en la ficha (T49a: clavos, su límite y la perturbación de la Defensa espiritual). Las
/// listas viven en <see cref="MistbornData"/> y la tabla de progresión en <see cref="ArtesMetalicas"/>.
/// </summary>
public sealed class MistbornRules : IWorldRules
{
    // Valores de CaminoInicial (decisión (k), Q7): "" = no decidido.
    private static readonly HashSet<string> CaminosIniciales = ["", "heroico", "metal"];

    // Claves de Recursos (§2) y de DerivadosSet que las acciones de mesa leen o escriben.
    private const string ClaveInvestidura = "investiduraActual";
    private const string ClaveCuentasAtium = "cuentasAtium";
    private const string ClaveArquillas = "arquillas";
    private const string ClaveCargasMaxArte = "feruquimia.cargasMax";
    private const string Atium = "atium";

    // Hemalurgia (T49a): tantos clavos implantados como el rango, máximo 3 (L.289 / PDF 295).
    private const int MaxClavos = 3;
    private const string ConceptoGradoClavo = "Clavo hemalúrgico (poder existente)";

    public string Id => WorldIds.Mistborn;

    /// <summary>La era es obligatoria: <c>era1</c> o <c>era2</c> (L.372 / PDF 378); cualquier otro valor da 400.</summary>
    public string? NormalizarEra(string? era) =>
        EraIds.EsValida(era) ? era : throw new ArgumentException($"Invalid Era: '{era}'.");

    /// <summary>Solo las reglas de Scadrial (Resistencia koloss).</summary>
    public IReadOnlyDictionary<string, List<ReglaTalento>> ReglasPropias => MistbornData.Reglas;

    /// <summary>Núcleo Cosmere (en el orden del literal) + Resistencia koloss; sin las reglas de Roshar.</summary>
    public IReadOnlyDictionary<string, List<ReglaTalento>> ReglasTalentos { get; } = TalentosReglas.Efectivas(MistbornData.Reglas);

    /// <summary>Investidura actual (L.26 / PDF 32), cuentas de atium (L.176 / PDF 182) y arquillas (L.254 / PDF 260).</summary>
    public IReadOnlySet<string> RecursosPermitidos { get; } = new HashSet<string> { ClaveInvestidura, ClaveCuentasAtium, ClaveArquillas };

    /// <summary>Q6: por ahora solo el director cambia el camino de nacido del metal (el libro lo permitiría, L.128 / PDF 134).</summary>
    public bool CaminoInvestidoLoCambiaElDirector => true;

    /// <summary>La Bendición de la Fortaleza (Desvío +1) se suma a la armadura [inferido, L.35 / PDF 41 → Q8].</summary>
    public bool DesvioBonoSeAcumula => true;

    // ── Identidad ────────────────────────────────────────────────────────────

    /// <summary>
    /// §5.3 (L.18-19 / PDF 24-25; L.32-39 / PDF 38-45; L.288-295 / PDF 294-301). Recibe la lista de poderes ya fusionada y
    /// normalizada (<c>CharacterJson.FusionarPoderes</c>) y un <c>CaminoInicial</c> ya coherente con los caminos
    /// (<c>CharacterService</c>), así que solo rechaza valores fuera de dominio. La existencia de <c>MetaId</c> la comprueba
    /// <c>CharacterService</c> (las reglas no tienen BD).
    /// </summary>
    public void ValidarIdentidad(IdentidadPersonaje id)
    {
        if (!string.IsNullOrEmpty(id.CaminoHeroico) && !MistbornData.CaminosHeroicos.Contains(id.CaminoHeroico))
            throw new ArgumentException($"Invalid CaminoHeroico: '{id.CaminoHeroico}'.");
        // Una campaña de Nacidos de la bruma nunca acumula datos de Roshar (decisión (j)).
        if (!string.IsNullOrEmpty(id.CaminoRadiante))
            throw new ArgumentException($"Invalid CaminoRadiante: '{id.CaminoRadiante}'.");
        if (!string.IsNullOrEmpty(id.CaminoMetal) && !MistbornData.CaminosMetal.Contains(id.CaminoMetal))
            throw new ArgumentException($"Invalid CaminoMetal: '{id.CaminoMetal}'.");
        if (id.CaminoInicial is null || !CaminosIniciales.Contains(id.CaminoInicial))
            throw new ArgumentException($"Invalid CaminoInicial: '{id.CaminoInicial}'.");
        if (!string.IsNullOrEmpty(id.Ascendencia) && !MistbornData.Ascendencias.Contains(id.Ascendencia))
            throw new ArgumentException($"Invalid Ascendencia: '{id.Ascendencia}'.");

        // Los kandra no pueden tomar talentos de nacido del metal (L.18 / PDF 24); los talentos principales de nacido de la
        // bruma y feruquimista exigen ascendencia humana (L.141 / PDF 147; L.146 / PDF 152).
        if (id.Ascendencia == MistbornData.Kandra && !string.IsNullOrEmpty(id.CaminoMetal))
            throw new ArgumentException($"Invalid CaminoMetal for Ascendencia '{id.Ascendencia}': '{id.CaminoMetal}'.");
        if (id.Ascendencia == MistbornData.SangreKoloss && id.CaminoMetal is not null && MistbornData.CaminosSoloHumanos.Contains(id.CaminoMetal))
            throw new ArgumentException($"Invalid CaminoMetal for Ascendencia '{id.Ascendencia}': '{id.CaminoMetal}'.");

        var vistos = new HashSet<(string, string)>();
        foreach (var p in id.Poderes)
        {
            if (p.Arte is null || !MistbornData.Artes.Contains(p.Arte))
                throw new ArgumentException($"Invalid Arte in poderes: '{p.Arte}'.");
            if (p.Metal is null || !MistbornData.Metales.Contains(p.Metal))
                throw new ArgumentException($"Invalid Metal in poderes: '{p.Metal}'.");
            if (p.Origen is null || !MistbornData.Origenes.Contains(p.Origen))
                throw new ArgumentException($"Invalid Origen in poderes: '{p.Origen}'.");
            if (!vistos.Add((p.Arte, p.Metal)))
                throw new ArgumentException($"Duplicate poder: '{p.Arte}:{p.Metal}'.");
            // La aleación de lerasium da el poder alomántico del metal aleado (L.295 / PDF 301).
            if (p.Origen == "lerasium" && (p.Arte != MistbornData.Alomancia || p.Metal == "atium"))
                throw new ArgumentException($"Invalid poder for Origen 'lerasium': '{p.Arte}:{p.Metal}'.");
            // Los medallones son solo feruquímicos, sin atium y con nicrosil no disponible para PJ (L.293-294 / PDF 299-300).
            if (p.Origen == "medallon" && (p.Arte != MistbornData.Feruquimia || (p.Metal is "atium" or "nicrosil")))
                throw new ArgumentException($"Invalid poder for Origen 'medallon': '{p.Arte}:{p.Metal}'.");
        }

        // Bendiciones kandra: una al crear y una segunda distinta en rango 3 (L.34-35 / PDF 40-41).
        foreach (var b in id.Bendiciones)
        {
            if (b is null || !MistbornData.Bendiciones.ContainsKey(b))
                throw new ArgumentException($"Invalid Bendicion: '{b}'.");
        }
        if (id.Bendiciones.Count > 2)
            throw new ArgumentException($"Invalid Bendiciones: {id.Bendiciones.Count} blessings (at most 2).");
        if (id.Bendiciones.Distinct().Count() != id.Bendiciones.Count)
            throw new ArgumentException($"Invalid Bendiciones: duplicate blessing in [{string.Join(", ", id.Bendiciones)}].");
        if (id.Bendiciones.Count > 0 && id.Ascendencia != MistbornData.Kandra)
            throw new ArgumentException($"Invalid Bendiciones for Ascendencia '{id.Ascendencia}': only a kandra has blessings.");

        // Clavos hemalúrgicos (T49a; L.288-291 / PDF 294-297): el metal es uno de los 12 de la tabla y el poder elegido, si lo hay,
        // es uno de los cuatro de su metal; un clavo de atributo no otorga poder. Sin tope de clavos: el límite (rango, máximo 3)
        // lo aconseja el cliente con hemalurgia.clavosMax (P6), como los presupuestos.
        foreach (var clavo in id.Clavos ?? [])
        {
            if (clavo is null)
                throw new ArgumentException("Invalid Clavos: null entry.");
            if (clavo.MetalClavo is null || !MistbornData.EsMetalDeClavo(clavo.MetalClavo))
                throw new ArgumentException($"Invalid MetalClavo in clavos: '{clavo.MetalClavo}'.");
            if (string.IsNullOrEmpty(clavo.PoderElegido)) continue;
            if (!MistbornData.ClavosPoder.TryGetValue(clavo.MetalClavo, out var tipo))
                throw new ArgumentException($"Invalid PoderElegido for MetalClavo '{clavo.MetalClavo}': '{clavo.PoderElegido}' (an attribute spike grants no power).");
            if (!tipo.Poderes.Contains(clavo.PoderElegido))
                throw new ArgumentException($"Invalid PoderElegido for MetalClavo '{clavo.MetalClavo}': '{clavo.PoderElegido}'.");
        }

        foreach (var (clave, valor) in id.Recursos)
        {
            if (!RecursosPermitidos.Contains(clave))
                throw new ArgumentException($"Invalid Recursos key: '{clave}'.");
            if (valor < 0)
                throw new ArgumentException($"Invalid Recursos value for '{clave}': {valor}.");
        }
    }

    /// <summary>
    /// Bloqueo de un jugador no GM en el <c>PUT</c> (§5.2), tras el del núcleo (<c>Name</c>, <c>CaminoHeroico</c>):
    /// <c>CaminoMetal</c> y <c>CaminoInicial</c> solo los cambia el director (Q6, Q7); la primera Bendición la fija el jugador y
    /// cualquier otro cambio es del director (Q16; la segunda es una recompensa de rango 3, L.35 / PDF 41); un no-GM no añade ni
    /// quita poderes (clavos, aleación de lerasium y medallones son recompensas del DJ, L.288 / PDF 294): los guardados que falten
    /// en su lista (copia cacheada de Bolsa o Talentos) se reinyectan y de los existentes solo cambia <c>MetaId</c>. Los clavos
    /// hemalúrgicos (T49a) también son una recompensa del DJ: se ignoran los del cuerpo y se conservan los guardados.
    /// </summary>
    public void RestringirCambiosNoGm(UpdateCharacterRequest request, CharacterEntity character)
    {
        request.CaminoMetal = character.CaminoMetal;
        request.CaminoInicial = character.CaminoInicial;
        request.Clavos = null;

        if (character.Bendiciones.Count > 0) request.Bendiciones = null;
        else if (request.Bendiciones is { Count: > 1 }) throw new UnauthorizedAccessException("Only the GM can grant a second blessing.");

        if (request.Poderes is not null)
        {
            if (request.Poderes.Any(p => p is null)) throw new ArgumentException("Invalid Poderes: null entry.");
            var existentes = CharacterJson.ParsePoderes(character.Poderes);
            if (request.Poderes.Any(p => !existentes.Any(e => e.Arte == p.Arte && e.Metal == p.Metal)))
                throw new UnauthorizedAccessException("Only the GM can add powers.");
            // El origen de un poder lo decide el director (si no, cambiar "camino" por "clavo" lo completaría al normalizar).
            foreach (var p in request.Poderes)
                p.Origen = existentes.First(e => e.Arte == p.Arte && e.Metal == p.Metal).Origen;
            foreach (var e in existentes.Where(e => !request.Poderes.Any(p => p.Arte == e.Arte && p.Metal == e.Metal)))
                request.Poderes.Add(e);
        }
    }

    // ── Estado de mesa (T13) ─────────────────────────────────────────────────

    /// <summary>
    /// Acciones de mesa (§5.2; L.129-130 / PDF 135-136): <c>PATCH recursos</c>, Beber vial e inicio de escena. <c>CharacterService</c>
    /// solo abre la transacción, comprueba permisos y guarda; las reglas están aquí. <paramref name="estado"/> es el
    /// <c>MapToResponse</c> previo: aporta la Investidura máxima y las cargas de los poderes. Todo se calcula sobre copias y se
    /// escribe en <paramref name="c"/> al final, así que un error (400, 403, 404) no deja cambios a medias.
    /// </summary>
    public void AplicarAccionMesa(CharacterEntity c, AccionMesa accion, CharacterResponse estado)
    {
        switch (accion)
        {
            case AccionMesa.PatchRecursos patch: PatchRecursos(c, patch.Cuerpo, patch.EsGm, estado); break;
            case AccionMesa.BeberVial vial: BeberVial(c, vial.Metales, estado); break;
            case AccionMesa.InicioEscena escena: InicioEscena(c, escena.Sorprendido, estado); break;
            default: throw new ArgumentException("Table action not available in this world.");
        }
    }

    /// <summary>
    /// <c>PATCH …/recursos</c>: solo se escribe lo no nulo. Claves de recurso fuera de <see cref="RecursosPermitidos"/> → 400 y un
    /// poder <c>(Arte, Metal)</c> que el personaje no tiene → 404 (<see cref="KeyNotFoundException"/>).
    /// </summary>
    private void PatchRecursos(CharacterEntity c, RecursosRequest cuerpo, bool esGm, CharacterResponse estado)
    {
        var recursos = CharacterJson.ParseRecursos(c.Recursos);
        var poderes = CharacterJson.ParsePoderes(c.Poderes);

        if (cuerpo.Recursos is not null)
        {
            foreach (var (clave, valor) in cuerpo.Recursos)
            {
                if (!RecursosPermitidos.Contains(clave)) throw new ArgumentException($"Invalid Recursos key: '{clave}'.");
                recursos[clave] = ValorDeRecurso(clave, valor, estado);
            }
        }

        if (cuerpo.Poderes is not null)
        {
            var talentos = TalentosDe(c);
            foreach (var cambio in cuerpo.Poderes)
            {
                if (cambio is null) throw new ArgumentException("Invalid Poderes: null entry.");
                var poder = poderes.FirstOrDefault(p => p.Arte == cambio.Arte && p.Metal == cambio.Metal)
                            ?? throw new KeyNotFoundException($"Poder not found: '{cambio.Arte}:{cambio.Metal}'.");
                AplicarCambioDePoder(poder, cambio, esGm, estado, talentos);
            }
        }

        // Solo se reescribe la columna cuya parte del cuerpo llegó.
        if (cuerpo.Recursos is not null) c.Recursos = CharacterJson.SerializarRecursos(recursos);
        if (cuerpo.Poderes is not null) c.Poderes = CharacterJson.SerializarPoderes(poderes);
    }

    /// <summary>
    /// Valor que se guarda de un recurso. <c>investiduraActual</c>: entero recortado a [0, Investidura máxima] (L.26 / PDF 32);
    /// <c>cuentasAtium</c>: entero ≥ 0, aparte de la Investidura (L.176 / PDF 182); <c>arquillas</c>: ≥ 0 con 2 decimales como
    /// máximo (óbolo 0,01 ar; L.254 / PDF 260). Un valor fuera de dominio da 400 (no se redondea ni se trunca).
    /// </summary>
    private static decimal ValorDeRecurso(string clave, decimal valor, CharacterResponse estado)
    {
        switch (clave)
        {
            case ClaveInvestidura:
                SoloEntero(clave, valor);
                return Math.Clamp(valor, 0m, InvestiduraMaxima(estado));
            case ClaveCuentasAtium:
                SoloEntero(clave, valor);
                NoNegativo(clave, valor);
                return valor;
            case ClaveArquillas:
                NoNegativo(clave, valor);
                if (decimal.Round(valor, 2) != valor)
                    throw new ArgumentException(FormattableString.Invariant($"Invalid Recursos value for '{clave}': {valor} (at most 2 decimals)."));
                return valor;
            default:
                throw new ArgumentException($"Invalid Recursos key: '{clave}'.");
        }
    }

    // Los mensajes llevan el valor recibido con formato invariable (sin coma decimal según la cultura del servidor).
    private static void SoloEntero(string clave, decimal valor)
    {
        if (decimal.Truncate(valor) != valor)
            throw new ArgumentException(FormattableString.Invariant($"Invalid Recursos value for '{clave}': {valor} (must be an integer)."));
    }

    private static void NoNegativo(string clave, decimal valor)
    {
        if (valor < 0) throw new ArgumentException(FormattableString.Invariant($"Invalid Recursos value for '{clave}': {valor}."));
    }

    /// <summary>
    /// Estado de mesa de un poder. Orden de aplicación: <c>completo</c>, <c>ajusteCargasMax</c>, <c>cargas</c>, <c>viales</c> y
    /// <c>desprovisto</c>; las cargas se recortan al máximo que resulta de los cambios anteriores de la misma petición.
    /// </summary>
    private static void AplicarCambioDePoder(PoderPersonaje p, PoderRecursosRequest cambio, bool esGm, CharacterResponse estado,
        IReadOnlyList<string> talentos)
    {
        // Las cargas que veía el jugador antes del cambio (el recorte de salida de MapToResponse).
        var cargasAntes = Math.Clamp(p.Cargas, 0, CargasMaxDe(p, estado, talentos));

        // Marcar la meta como completada a mano es un atajo de mesa (Q14). Un poder que no viene del camino y la alomancia de
        // atium nacen completos y no pueden volver a nacientes (L.290 / PDF 296; L.295 / PDF 301; L.177 / PDF 183).
        if (cambio.Completo is bool completo) p.Completo = completo || NaceCompleto(p);

        if (cambio.AjusteCargasMax is int ajuste)
        {
            // Componedor: -1 carga máxima permanente por uso, sin bajar de cero (L.155 / PDF 161).
            if (ajuste > 0)
                throw new ArgumentException($"Invalid AjusteCargasMax for '{p.Arte}:{p.Metal}': {ajuste} (must be 0 or negative).");
            p.AjusteCargasMax = 0;
            p.AjusteCargasMax = Math.Max(ajuste, -CargasMaxDe(p, estado, talentos));
        }

        if (cambio.Cargas is int cargas)
        {
            int nuevas;
            if (p.Arte != MistbornData.Feruquimia || !p.Completo)
            {
                // Sin mente de metal no hay dónde almacenar: la alomancia no usa cargas y un poder feruquímico naciente todavía no
                // tiene mente de metal (L.162 / PDF 168; los poderes que no vienen del camino nacen completos).
                if (cargas > 0)
                    throw new ArgumentException(p.Arte != MistbornData.Feruquimia
                        ? $"Invalid Cargas for '{p.Arte}:{p.Metal}': only feruquimia powers store charges."
                        : $"Invalid Cargas for '{p.Arte}:{p.Metal}': a nascent power has no metalmind and stores no charges.");
                nuevas = 0;
            }
            else nuevas = Math.Clamp(cargas, 0, CargasMaxDe(p, estado, talentos));

            // Almacenar en un medallón no genera cargas y se recarga sustituyéndolo: el jugador solo las gasta (L.293 / PDF 299).
            if (!esGm && p.Origen == "medallon" && nuevas > cargasAntes)
                throw new UnauthorizedAccessException("Only the GM can add charges to a medallion.");
            p.Cargas = nuevas;
        }
        else if (cambio.AjusteCargasMax is not null)
        {
            // El máximo ha bajado: las cargas guardadas no pueden superarlo.
            p.Cargas = Math.Clamp(p.Cargas, 0, CargasMaxDe(p, estado, talentos));
        }

        if (cambio.Viales is int viales)
        {
            if (viales < 0) throw new ArgumentException($"Invalid Viales for '{p.Arte}:{p.Metal}': {viales}.");
            p.Viales = viales;
        }

        if (cambio.Desprovisto is bool desprovisto) p.Desprovisto = desprovisto;
    }

    private static bool NaceCompleto(PoderPersonaje p) =>
        p.Origen != "camino" || (p.Arte == MistbornData.Alomancia && p.Metal == Atium);

    /// <summary>
    /// Cargas máximas de la mente de metal de un poder con su estado actual: las mismas que <see cref="Derivar"/> emite en
    /// <c>poder.&lt;metal&gt;.cargasMax</c> (0 si es naciente o no es feruquimia; 8 en un medallón). Se recalcula con
    /// <see cref="CargasMaxPoder"/> a partir de las líneas del arte de <paramref name="estado"/> porque el poder puede haber
    /// cambiado (<c>completo</c>, <c>ajusteCargasMax</c>) en la misma petición.
    /// </summary>
    private static int CargasMaxDe(PoderPersonaje p, CharacterResponse estado, IReadOnlyList<string> talentos) =>
        p.Arte == MistbornData.Feruquimia && estado.DerivadosSet.TryGetValue(ClaveCargasMaxArte, out var arte)
            ? (int)Math.Round(CargasMaxPoder(p, arte.Lineas, talentos).Total)
            : 0;

    /// <summary>Talentos del personaje como los ve <c>MapToResponse</c>: los guardados más los implícitos de la ascendencia.</summary>
    private IReadOnlyList<string> TalentosDe(CharacterEntity c)
    {
        List<string> guardados = [];
        if (!string.IsNullOrWhiteSpace(c.Talentos))
        {
            try { guardados = JsonSerializer.Deserialize<List<string>>(c.Talentos) ?? []; }
            catch (JsonException) { /* tolerante, como CharacterService.ParseTalentos */ }
        }
        return guardados.Union(TalentosImplicitos(c)).ToList();
    }

    private static decimal InvestiduraMaxima(CharacterResponse estado) =>
        Math.Max(0m, (decimal)Math.Round(estado.Investidura.Total));

    /// <summary>
    /// Beber vial (L.129-130 / PDF 135-136; Q17): <paramref name="metales"/> es el contenido del vial tal cual lo manda el cliente
    /// (los 8 comunes preseleccionados, pero desmarcables). Cada poder alomántico, salvo el de atium, queda Desprovisto si su
    /// metal no está en el vial y deja de estarlo si está; por cada metal raro del vial con <c>Viales &gt; 0</c> se gasta un vial
    /// (sin 409: el recuento es discreción del DJ). La Investidura actual sube al máximo solo si el vial contiene un metal que el
    /// personaje puede quemar. El atium queda fuera de la regla: no da Investidura y sus cuentas van aparte (L.176 / PDF 182).
    /// </summary>
    private void BeberVial(CharacterEntity c, IReadOnlyList<string> metales, CharacterResponse estado)
    {
        foreach (var metal in metales)
        {
            if (metal is null || !MistbornData.Metales.Contains(metal))
                throw new ArgumentException($"Invalid Metal in vial: '{metal}'.");
            if (metal == Atium)
                throw new ArgumentException("Invalid Metal in vial: 'atium' is not part of a vial (track it with recursos.cuentasAtium).");
        }

        var poderes = CharacterJson.ParsePoderes(c.Poderes);
        if (!poderes.Any(p => p.Arte == MistbornData.Alomancia))
            throw new ArgumentException("This character has no alomantic powers.");

        var delVial = metales.ToHashSet();
        var quemables = poderes.Where(p => p.Arte == MistbornData.Alomancia && p.Metal != Atium).ToList();
        foreach (var p in quemables)
        {
            var enElVial = delVial.Contains(p.Metal);
            p.Desprovisto = !enElVial;
            if (enElVial && !MistbornData.MetalesComunes.Contains(p.Metal) && p.Viales > 0) p.Viales--;
        }

        if (quemables.Any(p => delVial.Contains(p.Metal)))
        {
            var recursos = CharacterJson.ParseRecursos(c.Recursos);
            recursos[ClaveInvestidura] = InvestiduraMaxima(estado);
            c.Recursos = CharacterJson.SerializarRecursos(recursos);
        }
        c.Poderes = CharacterJson.SerializarPoderes(poderes);
    }

    /// <summary>
    /// Inicio de escena (L.129 / PDF 135): la Investidura actual empieza al máximo, o en 1 si empiezas Sorprendido. Sin Sorprendido
    /// se da por bebido un vial por instinto, así que además se limpia el Desprovisto de los poderes alománticos de metal común
    /// (los que contiene un vial estándar, L.130 / PDF 136) y los raros conservan su estado, que gobiernan los <c>viales</c>
    /// [inferido → Q24]. Con Sorprendido no se toca ningún Desprovisto. 400 si el personaje no tiene Investidura.
    /// </summary>
    private void InicioEscena(CharacterEntity c, bool sorprendido, CharacterResponse estado)
    {
        if (!RecursosPermitidos.Contains(ClaveInvestidura))
            throw new ArgumentException($"This world has no '{ClaveInvestidura}' resource.");
        var total = InvestiduraMaxima(estado);
        if (total == 0) throw new ArgumentException("This character has no Investiture.");

        var recursos = CharacterJson.ParseRecursos(c.Recursos);
        recursos[ClaveInvestidura] = Math.Min(sorprendido ? 1m : total, total);
        c.Recursos = CharacterJson.SerializarRecursos(recursos);

        if (sorprendido) return;
        var poderes = CharacterJson.ParsePoderes(c.Poderes);
        foreach (var p in poderes.Where(p => p.Arte == MistbornData.Alomancia && p.Metal != Atium && MistbornData.MetalesComunes.Contains(p.Metal)))
            p.Desprovisto = false;
        c.Poderes = CharacterJson.SerializarPoderes(poderes);
    }

    // ── Metas de nacido del metal ────────────────────────────────────────────

    /// <summary>
    /// Concluir la meta (con cualquier tipo de conclusión, Q20) da su recompensa: la versión completa de los poderes que la enlazan
    /// (L.132-133 / PDF 138-139; L.284 / PDF 290). Sin efecto si ningún poder la enlaza.
    /// </summary>
    public void AlConcluirMeta(CharacterEntity c, MetaEntity meta)
    {
        var poderes = CharacterJson.ParsePoderes(c.Poderes);
        if (!poderes.Any(p => p.MetaId == meta.Id)) return;
        foreach (var p in poderes.Where(p => p.MetaId == meta.Id)) p.Completo = true;
        c.Poderes = CharacterJson.SerializarPoderes(poderes);
    }

    /// <summary>Al borrar la meta, los poderes que la enlazaban quedan sin meta (<c>Completo</c> no cambia).</summary>
    public void AlBorrarMeta(CharacterEntity c, MetaEntity meta)
    {
        var poderes = CharacterJson.ParsePoderes(c.Poderes);
        if (!poderes.Any(p => p.MetaId == meta.Id)) return;
        foreach (var p in poderes.Where(p => p.MetaId == meta.Id)) p.MetaId = null;
        c.Poderes = CharacterJson.SerializarPoderes(poderes);
    }

    // ── Cálculo ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Talentos de ascendencia que el cliente concede solos (<c>autoGranted</c>) y no guarda: Resistencia koloss (L.38 / PDF 44),
    /// Forma natural y Disfraz kandra (L.34 / PDF 40).
    /// </summary>
    public IEnumerable<string> TalentosImplicitos(CharacterEntity c) => c.Ascendencia switch
    {
        MistbornData.SangreKoloss => ["Resistencia koloss"],
        MistbornData.Kandra => ["Forma natural", "Disfraz kandra"],
        _ => [],
    };

    /// <summary>
    /// Investidura con un camino alomántico (L.26 / PDF 32; L.129 / PDF 135) o con un poder alomántico de clavo o de aleación de
    /// lerasium (L.290 / PDF 296; L.295 / PDF 301). La feruquimia no usa Investidura (L.131 / PDF 137): v1 no la concede por un
    /// poder feruquímico [inferido → Q19].
    /// </summary>
    public bool TieneInvestidura(CharacterEntity c, IReadOnlyList<string> talentos, IReadOnlyList<PoderPersonaje> poderes) =>
        MistbornData.CaminosAlomanticos.Contains(c.CaminoMetal)
        || poderes.Any(p => p.Arte == MistbornData.Alomancia && (p.Origen is "clavo" or "lerasium"));

    /// <summary>
    /// Bonos permanentes de atributo: Bendiciones kandra (L.34-35 / PDF 40-41), Tamaño desmedido (L.39 / PDF 45), Guardián del
    /// conocimiento (L.229 / PDF 235) y clavos hemalúrgicos de atributo implantados (+1 por clavo, acumulables por metal;
    /// L.288 y L.291 / PDF 294 y 297). El origen (concepto de las líneas de bono) es el nombre de cada fuente, separados por «, ».
    /// </summary>
    public BonosForma BonosAtributos(CharacterEntity c, IReadOnlyList<string> talentos, out string? origen)
    {
        int fuerza = 0, velocidad = 0, intelecto = 0, voluntad = 0, discernimiento = 0, presencia = 0, desvio = 0;
        var fuentes = new List<string>();
        void Sumar(string nombre, BonosForma b, int veces = 1)
        {
            fuerza += b.Fuerza * veces; velocidad += b.Velocidad * veces; intelecto += b.Intelecto * veces; voluntad += b.Voluntad * veces;
            discernimiento += b.Discernimiento * veces; presencia += b.Presencia * veces; desvio += b.Desvio * veces;
            fuentes.Add(nombre);
        }

        foreach (var id in c.Bendiciones.Distinct())
        {
            if (id is not null && MistbornData.Bendiciones.TryGetValue(id, out var bendicion)) Sumar(bendicion.Nombre, bendicion.Bonos);
        }
        foreach (var (talento, bonos) in MistbornData.TalentosConBonoAtributo)
        {
            if (talentos.Contains(talento)) Sumar(talento, bonos);
        }
        // Clavos de atributo implantados (T49a): cinc Voluntad, cobre Intelecto, estaño Discernimiento y hierro Fuerza, +1 cada
        // uno; los del mismo metal se acumulan (dos clavos de hierro: Fuerza +2; L.288 / PDF 294). Un clavo de poder no sube ningún
        // atributo.
        foreach (var grupo in ClavosImplantados(c).Where(k => MistbornData.ClavosAtributo.ContainsKey(k.MetalClavo)).GroupBy(k => k.MetalClavo))
        {
            var tipo = MistbornData.ClavosAtributo[grupo.Key];
            var cantidad = grupo.Count();
            Sumar(cantidad > 1 ? $"{tipo.Nombre} ×{cantidad}" : tipo.Nombre, tipo.Bono, cantidad);
        }

        origen = fuentes.Count > 0 ? string.Join(", ", fuentes) : null;
        return new BonosForma(fuerza, velocidad, intelecto, voluntad, discernimiento, presencia, desvio);
    }

    /// <summary>Sin el prefijo «Forma: » de Stormlight: «Bendición de la Consciencia», «Tamaño desmedido»…</summary>
    public string EtiquetaBono(string? origen) => origen ?? "Bendición";

    /// <summary>
    /// Derivados de las artes metálicas (§6.3; tabla de L.163 / PDF 169). Se emite el arte A si existe el hueco de su habilidad
    /// Investida (<c>Alomancia</c> con Voluntad, <c>Feruquimia</c> con Intelecto, L.128 / PDF 134), si el camino la implica o si
    /// algún poder es de A (clavo, aleación de lerasium o medallón sin camino: L.290 / PDF 296; L.295 / PDF 301). Sin hueco, los
    /// grados son 0 (límite 1, dado 1 «sin tirada», alcance 3 m). Sin hueco, sin camino y sin poderes: diccionario vacío.
    /// </summary>
    public Dictionary<string, StatDesglose> Derivar(CharacterEntity c, IReadOnlyList<string> talentos, IReadOnlyList<PoderPersonaje> poderes, BonosForma fb)
    {
        var d = new Dictionary<string, StatDesglose>();
        BonosAtributos(c, talentos, out var origen);
        var etiqueta = EtiquetaBono(origen);
        var rango = TalentosReglas.Rango(c.Level);

        var gradosAlomancia = TalentosReglas.GradosHabilidadPersonalizada(c, "Alomancia");
        var gradosFeruquimia = TalentosReglas.GradosHabilidadPersonalizada(c, "Feruquimia");
        var alomancia = gradosAlomancia is not null || MistbornData.CaminosAlomanticos.Contains(c.CaminoMetal)
                        || poderes.Any(p => p.Arte == MistbornData.Alomancia);
        var feruquimia = gradosFeruquimia is not null || MistbornData.CaminosFeruquimicos.Contains(c.CaminoMetal)
                         || poderes.Any(p => p.Arte == MistbornData.Feruquimia);

        // Hemalurgia (T49a): los clavos implantados que otorgan un poder que el personaje ya tenía suman un grado por arte.
        var clavos = CharacterJson.ParseClavos(c.Clavos);
        var gradosClavo = GradosExtraPorClavos(clavos, poderes);
        gradosClavo.TryGetValue(MistbornData.Alomancia, out var clavoAlomancia);
        gradosClavo.TryGetValue(MistbornData.Feruquimia, out var clavoFeruquimia);

        // Portentoso: un grado más solo para el alcance de los poderes alománticos (L.136 / PDF 142).
        if (alomancia)
            ArteDerivada(d, MistbornData.Alomancia, "Alomancia", gradosAlomancia ?? 0, clavoAlomancia, c.Voluntad, "Voluntad", fb.Voluntad,
                etiqueta, rango, gradoExtraAlcance: talentos.Contains("Portentoso"));
        if (feruquimia)
            ArteDerivada(d, MistbornData.Feruquimia, "Feruquimia", gradosFeruquimia ?? 0, clavoFeruquimia, c.Intelecto, "Intelecto", fb.Intelecto,
                etiqueta, rango, gradoExtraAlcance: false);

        // Cargas máximas de cada mente de metal: 2 + grados en Feruquimia (L.131 / PDF 137) + rango con Mentes de metal
        // ampliadas (L.146 / PDF 152).
        List<StatLinea> cargas =
        [
            new() { Concepto = "Base", Valor = 2 },
            new() { Concepto = "Grados en Feruquimia", Valor = gradosFeruquimia ?? 0 },
        ];
        if (clavoFeruquimia != 0) cargas.Add(LineaGradoClavo(clavoFeruquimia));
        if (talentos.Contains("Mentes de metal ampliadas")) cargas.Add(new() { Concepto = "Mentes de metal ampliadas", Valor = rango });
        if (feruquimia) d[ClaveCargasMaxArte] = Desglose(Copia(cargas));

        foreach (var p in poderes.Where(p => p.Arte == MistbornData.Feruquimia))
            d[$"poder.{p.Metal}.cargasMax"] = CargasMaxPoder(p, cargas, talentos);

        // Mentes que se pueden llevar a la vez: solo lo limita Herencia feruquímica (Intelecto, mínimo 1; con Ancho de banda
        // mental, el modificador de Feruquimia: L.146 / PDF 152). Ferrin y nacidoble: una mente por poder [inferido → Q27].
        if (c.CaminoMetal == "feruquimista")
        {
            List<StatLinea> mentes;
            if (talentos.Contains("Ancho de banda mental"))
                mentes = [new() { Concepto = "Ancho de banda mental", Valor = d["feruquimia.modificador"].Total }];
            else if (c.Intelecto + fb.Intelecto < 1)
                mentes = [new() { Concepto = "Mínimo", Valor = 1 }];
            else
            {
                mentes = [new() { Concepto = "Intelecto", Valor = c.Intelecto }];
                if (fb.Intelecto != 0) mentes.Add(new() { Concepto = etiqueta, Valor = fb.Intelecto, EsBono = true });
            }
            d["feruquimia.mentesALaVez"] = Desglose(mentes);
        }

        // Hemalurgia (T49a): tantos clavos implantados como el rango, máximo 3 (L.289 / PDF 295); solo si el personaje tiene algún clavo.
        if (clavos.Count > 0)
            d["hemalurgia.clavosMax"] = Desglose([new()
            {
                Concepto = rango > MaxClavos ? $"Rango (máximo {MaxClavos})" : "Rango",
                Valor = Math.Min(rango, MaxClavos),
            }]);

        return d;
    }

    /// <summary>
    /// Hemalurgia en la Defensa espiritual (L.290 / PDF 296): cada clavo implantado perturba la redespíritu, así que el primer
    /// clavo de cada metal reduce la Defensa espiritual en 2 y los adicionales del mismo metal, en 5 (una línea por clavo, de
    /// atributo o de poder). Al inicio de cada escena, con al menos un clavo y una Defensa espiritual de 9 o menos, el personaje
    /// queda Desorientado hasta el final de la escena: línea situacional, sin efecto en el total. Los demás desgloses no cambian.
    /// Las Bendiciones kandra no son clavos de la lista, así que no perturban nada (L.291 / PDF 297: sin otros clavos tampoco lo
    /// hacen); su interacción con otros clavos no se modela en v1.
    /// </summary>
    public void CompletarDesglose(CharacterEntity c, StatAfectada stat, StatDesglose d)
    {
        if (stat != StatAfectada.DefensaEspiritual) return;
        var clavos = ClavosImplantados(c);
        if (clavos.Count == 0) return;

        var porMetal = new Dictionary<string, int>();
        foreach (var clavo in clavos)
        {
            var orden = porMetal[clavo.MetalClavo] = porMetal.GetValueOrDefault(clavo.MetalClavo) + 1;
            var nombre = MistbornData.NombreClavo(clavo.MetalClavo);
            d.Lineas.Add(orden == 1
                ? new StatLinea
                {
                    Concepto = $"{nombre}: redespíritu perturbada", Valor = -2,
                    DescripcionCondicion = "El primer clavo de cada metal reduce la Defensa espiritual en 2 (L.290 / PDF 296)",
                }
                : new StatLinea
                {
                    Concepto = $"{nombre} adicional: redespíritu perturbada", Valor = -5,
                    DescripcionCondicion = "Cada clavo adicional del mismo metal la reduce en 5 (L.290 / PDF 296)",
                });
        }
        d.Total = d.Lineas.Sum(l => l.Valor);

        if (d.Total <= 9)
            d.Situacional.Add(new StatLinea
            {
                Concepto = "Desorientado al inicio de escena", Valor = 0, SinValor = true,
                DescripcionCondicion = "Con al menos un clavo y Defensa espiritual 9 o menos, quedas Desorientado hasta el final de la escena (L.290 / PDF 296)",
            });
    }

    /// <summary>
    /// Clavos implantados del personaje, en el orden de la lista: un clavo extraído conserva sus datos pero sus efectos terminan
    /// (L.290 / PDF 296). El clavo secreto (L.289 / PDF 295) es solo informativo en v1 y cuenta como cualquier otro.
    /// </summary>
    private static List<ClavoHemalurgico> ClavosImplantados(CharacterEntity c) =>
        CharacterJson.ParseClavos(c.Clavos).Where(k => k.Implantado && k.MetalClavo is not null).ToList();

    /// <summary>
    /// «Poder existente» (L.290 / PDF 296): un clavo implantado que otorga un poder que el personaje ya tenía da un grado adicional
    /// en la habilidad de su arte, que no cuenta para el máximo de grados (por eso se suma al derivar y no se guarda en el hueco de
    /// la habilidad). El primer clavo que elige un poder de origen «clavo» es el que lo otorga; los demás, y todo clavo que elige un
    /// poder de otro origen (camino, aleación de lerasium, medallón), dan el grado. Un clavo cuyo poder no figura en
    /// <paramref name="poderes"/> no da nada [inferido: el cliente envía el poder junto al clavo, T49b]. Devuelve los grados por arte.
    /// </summary>
    private static Dictionary<string, int> GradosExtraPorClavos(IReadOnlyList<ClavoHemalurgico> clavos, IReadOnlyList<PoderPersonaje> poderes)
    {
        var extra = new Dictionary<string, int>();
        foreach (var grupo in clavos.Where(k => k.Implantado && !string.IsNullOrEmpty(k.PoderElegido)).GroupBy(k => k.PoderElegido!))
        {
            var poder = poderes.FirstOrDefault(p => $"{p.Arte}:{p.Metal}" == grupo.Key);
            if (poder is null) continue;
            var grados = poder.Origen == "clavo" ? grupo.Count() - 1 : grupo.Count();
            if (grados > 0) extra[poder.Arte] = extra.GetValueOrDefault(poder.Arte) + grados;
        }
        return extra;
    }

    private static StatLinea LineaGradoClavo(int grados) => new()
    {
        Concepto = ConceptoGradoClavo, Valor = grados,
        DescripcionCondicion = "Un grado adicional que no cuenta para el máximo de grados (L.290 / PDF 296)",
    };

    /// <summary>
    /// Modificador, límite, dado y alcance de un arte (L.128 / PDF 134; L.163 / PDF 169). <paramref name="gradosClavo"/> son los
    /// grados adicionales de clavos que otorgan un poder que el personaje ya tenía (L.290 / PDF 296): cuentan para todo lo derivado,
    /// pero no para el máximo de grados (no se guardan en el hueco de la habilidad).
    /// </summary>
    private static void ArteDerivada(Dictionary<string, StatDesglose> d, string arte, string nombreArte, int grados, int gradosClavo,
        int atributo, string nombreAtributo, int bonoAtributo, string etiquetaBono, int rango, bool gradoExtraAlcance)
    {
        var gradosConcepto = $"Grados en {nombreArte}";
        var efectivos = grados + gradosClavo;

        List<StatLinea> modificador = [new() { Concepto = gradosConcepto, Valor = grados }];
        if (gradosClavo != 0) modificador.Add(LineaGradoClavo(gradosClavo));
        modificador.Add(new() { Concepto = nombreAtributo, Valor = atributo });
        if (bonoAtributo != 0) modificador.Add(new() { Concepto = etiquetaBono, Valor = bonoAtributo, EsBono = true });
        d[$"{arte}.modificador"] = Desglose(modificador);

        var fila = ArtesMetalicas.Progresion(efectivos, rango);
        // El límite son los grados (mínimo 1); con 6 o más, «igual al rango».
        List<StatLinea> limite;
        if (efectivos >= 6) limite = [new() { Concepto = "Rango", Valor = rango }];
        else if (efectivos <= 0) limite = [new() { Concepto = "Mínimo", Valor = 1 }];
        else
        {
            limite = [];
            if (grados != 0) limite.Add(new() { Concepto = gradosConcepto, Valor = grados });
            if (gradosClavo != 0) limite.Add(LineaGradoClavo(gradosClavo));
        }
        d[$"{arte}.limite"] = Desglose(limite);

        // Total = caras del dado (1 = «sin tirada»); el cliente lo pinta como d{total} o «—».
        d[$"{arte}.dado"] = Desglose([new() { Concepto = "Dado de artes metálicas", Valor = fila.DadoCaras }], "d");

        List<StatLinea> alcance = [new() { Concepto = $"{gradosConcepto} ({efectivos})", Valor = fila.AlcanceMetros }];
        if (gradoExtraAlcance)
        {
            var extra = ArtesMetalicas.Progresion(efectivos + 1, rango).AlcanceMetros - fila.AlcanceMetros;
            if (extra != 0) alcance.Add(new() { Concepto = "Portentoso", Valor = extra });
        }
        d[$"{arte}.alcance"] = Desglose(alcance, "m");
    }

    /// <summary>
    /// Cargas máximas de la mente de metal de un poder feruquímico: 0 mientras es naciente (no hay mente de metal, L.162 / PDF
    /// 168); 8 fijas en un medallón (L.293 / PDF 299); si no, las del arte + 5 en el cobre con Guardián del conocimiento
    /// (L.229 / PDF 235) + el ajuste permanente de Componedor (L.155 / PDF 161).
    /// </summary>
    private static StatDesglose CargasMaxPoder(PoderPersonaje p, List<StatLinea> cargasArte, IReadOnlyList<string> talentos)
    {
        if (!p.Completo) return Desglose([new() { Concepto = "Naciente: sin mente de metal", Valor = 0 }]);
        if (p.Origen == "medallon") return Desglose([new() { Concepto = "Medallón", Valor = 8 }]);

        var lineas = Copia(cargasArte);
        if (p.Metal == "cobre" && talentos.Contains("Guardián del conocimiento"))
            lineas.Add(new() { Concepto = "Guardián del conocimiento", Valor = 5 });
        if (p.AjusteCargasMax != 0) lineas.Add(new() { Concepto = "Componedor", Valor = p.AjusteCargasMax });
        var desglose = Desglose(lineas);
        desglose.Total = Math.Max(0, desglose.Total); // un máximo de cargas nunca es negativo
        return desglose;
    }

    private static StatDesglose Desglose(List<StatLinea> lineas, string? unidad = null) =>
        new() { Total = lineas.Sum(l => l.Valor), Unidad = unidad, Lineas = lineas, Situacional = [] };

    private static List<StatLinea> Copia(List<StatLinea> lineas) =>
        lineas.Select(l => new StatLinea { Concepto = l.Concepto, Valor = l.Valor, DescripcionCondicion = l.DescripcionCondicion, EsBono = l.EsBono }).ToList();
}
