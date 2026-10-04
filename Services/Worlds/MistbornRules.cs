using Messages.Characters;
using Messages.Characters.In;
using Messages.Characters.Out;
using Messages.Database.Entities;
using Messages.Worlds;
using Services.Characters;

namespace Services.Worlds;

/// <summary>
/// Reglas del mundo «Nacidos de la bruma» (Scadrial): validación de la identidad (§5.3), bloqueo de los cambios de un jugador no
/// GM (Q6, Q16), condición de Investidura, Resistencia koloss, Bendiciones kandra y talentos de atributo como bonos, y derivados de
/// las artes metálicas (<c>DerivadosSet</c>). Las listas viven en <see cref="MistbornData"/> y la tabla de progresión en
/// <see cref="ArtesMetalicas"/>. <c>AplicarAccionMesa</c> sigue delegando en Stormlight (400) hasta T13.
/// </summary>
public sealed class MistbornRules : IWorldRules
{
    private static readonly StormlightRules Respaldo = new(); // hasta T13: solo AplicarAccionMesa

    // Valores de CaminoInicial (decisión (k), Q7): "" = no decidido.
    private static readonly HashSet<string> CaminosIniciales = ["", "heroico", "metal"];

    public string Id => WorldIds.Mistborn;

    /// <summary>La era es obligatoria: <c>era1</c> o <c>era2</c> (L.372 / PDF 378); cualquier otro valor da 400.</summary>
    public string? NormalizarEra(string? era) =>
        EraIds.EsValida(era) ? era : throw new ArgumentException($"Invalid Era: '{era}'.");

    /// <summary>Solo las reglas de Scadrial (Resistencia koloss).</summary>
    public IReadOnlyDictionary<string, List<ReglaTalento>> ReglasPropias => MistbornData.Reglas;

    /// <summary>Núcleo Cosmere (en el orden del literal) + Resistencia koloss; sin las reglas de Roshar.</summary>
    public IReadOnlyDictionary<string, List<ReglaTalento>> ReglasTalentos { get; } = TalentosReglas.Efectivas(MistbornData.Reglas);

    /// <summary>Investidura actual (L.26 / PDF 32), cuentas de atium (L.176 / PDF 182) y arquillas (L.254 / PDF 260).</summary>
    public IReadOnlySet<string> RecursosPermitidos { get; } = new HashSet<string> { "investiduraActual", "cuentasAtium", "arquillas" };

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
    /// en su lista (copia cacheada de Bolsa o Talentos) se reinyectan y de los existentes solo cambia <c>MetaId</c>.
    /// </summary>
    public void RestringirCambiosNoGm(UpdateCharacterRequest request, CharacterEntity character)
    {
        request.CaminoMetal = character.CaminoMetal;
        request.CaminoInicial = character.CaminoInicial;

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

    public void AplicarAccionMesa(CharacterEntity c, AccionMesa accion, CharacterResponse estado) =>
        Respaldo.AplicarAccionMesa(c, accion, estado);

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
    /// Bonos permanentes de atributo: Bendiciones kandra (L.34-35 / PDF 40-41), Tamaño desmedido (L.39 / PDF 45) y Guardián del
    /// conocimiento (L.229 / PDF 235). El origen (concepto de las líneas de bono) es el nombre de cada fuente, separados por «, ».
    /// </summary>
    public BonosForma BonosAtributos(CharacterEntity c, IReadOnlyList<string> talentos, out string? origen)
    {
        int fuerza = 0, velocidad = 0, intelecto = 0, voluntad = 0, discernimiento = 0, presencia = 0, desvio = 0;
        var fuentes = new List<string>();
        void Sumar(string nombre, BonosForma b)
        {
            fuerza += b.Fuerza; velocidad += b.Velocidad; intelecto += b.Intelecto; voluntad += b.Voluntad;
            discernimiento += b.Discernimiento; presencia += b.Presencia; desvio += b.Desvio;
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

        // Portentoso: un grado más solo para el alcance de los poderes alománticos (L.136 / PDF 142).
        if (alomancia)
            ArteDerivada(d, MistbornData.Alomancia, "Alomancia", gradosAlomancia ?? 0, c.Voluntad, "Voluntad", fb.Voluntad,
                etiqueta, rango, gradoExtraAlcance: talentos.Contains("Portentoso"));
        if (feruquimia)
            ArteDerivada(d, MistbornData.Feruquimia, "Feruquimia", gradosFeruquimia ?? 0, c.Intelecto, "Intelecto", fb.Intelecto,
                etiqueta, rango, gradoExtraAlcance: false);

        // Cargas máximas de cada mente de metal: 2 + grados en Feruquimia (L.131 / PDF 137) + rango con Mentes de metal
        // ampliadas (L.146 / PDF 152).
        List<StatLinea> cargas =
        [
            new() { Concepto = "Base", Valor = 2 },
            new() { Concepto = "Grados en Feruquimia", Valor = gradosFeruquimia ?? 0 },
        ];
        if (talentos.Contains("Mentes de metal ampliadas")) cargas.Add(new() { Concepto = "Mentes de metal ampliadas", Valor = rango });
        if (feruquimia) d["feruquimia.cargasMax"] = Desglose(Copia(cargas));

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

        return d;
    }

    /// <summary>Modificador, límite, dado y alcance de un arte (L.128 / PDF 134; L.163 / PDF 169).</summary>
    private static void ArteDerivada(Dictionary<string, StatDesglose> d, string arte, string nombreArte, int grados, int atributo,
        string nombreAtributo, int bonoAtributo, string etiquetaBono, int rango, bool gradoExtraAlcance)
    {
        var gradosConcepto = $"Grados en {nombreArte}";

        List<StatLinea> modificador =
        [
            new() { Concepto = gradosConcepto, Valor = grados },
            new() { Concepto = nombreAtributo, Valor = atributo },
        ];
        if (bonoAtributo != 0) modificador.Add(new() { Concepto = etiquetaBono, Valor = bonoAtributo, EsBono = true });
        d[$"{arte}.modificador"] = Desglose(modificador);

        var fila = ArtesMetalicas.Progresion(grados, rango);
        // El límite son los grados (mínimo 1); con 6 o más, «igual al rango».
        StatLinea limite = grados >= 6 ? new() { Concepto = "Rango", Valor = rango }
            : grados <= 0 ? new() { Concepto = "Mínimo", Valor = 1 }
            : new() { Concepto = gradosConcepto, Valor = grados };
        d[$"{arte}.limite"] = Desglose([limite]);

        // Total = caras del dado (1 = «sin tirada»); el cliente lo pinta como d{total} o «—».
        d[$"{arte}.dado"] = Desglose([new() { Concepto = "Dado de artes metálicas", Valor = fila.DadoCaras }], "d");

        List<StatLinea> alcance = [new() { Concepto = $"{gradosConcepto} ({grados})", Valor = fila.AlcanceMetros }];
        if (gradoExtraAlcance)
        {
            var extra = ArtesMetalicas.Progresion(grados + 1, rango).AlcanceMetros - fila.AlcanceMetros;
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
