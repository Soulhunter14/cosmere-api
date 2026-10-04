using System.Diagnostics;
using Services.Characters;

namespace Services.Worlds;

/// <summary>
/// Datos de reglas del mundo «Nacidos de la bruma» (Scadrial) que usa <see cref="MistbornRules"/>: ids de caminos, ascendencias,
/// artes, metales, orígenes de poder y Bendiciones kandra, y las reglas de talento propias del mundo. Los ids son los del frontend
/// (<c>src/data/mistborn/metales.ts</c>; P5: los datos se duplican, la lógica no). <c>CharacterService</c> nunca lo cita: solo
/// llama a los miembros de <see cref="IWorldRules"/>.
/// </summary>
public static class MistbornData
{
    public const string Alomancia = "alomancia";
    public const string Feruquimia = "feruquimia";

    public const string Humano = "Humano";
    public const string Kandra = "Kandra";
    public const string SangreKoloss = "Sangre koloss";

    /// <summary>
    /// Los 6 caminos heroicos (L.19 / PDF 25): son del núcleo Cosmere y coinciden con los de Stormlight; el dato se duplica aquí
    /// porque la lista de <c>StormlightRules</c> es privada.
    /// </summary>
    public static readonly IReadOnlySet<string> CaminosHeroicos = new HashSet<string>
    {
        "agente", "cazador", "enviado", "erudito", "guerrero", "lider",
    };

    /// <summary>Caminos de nacido del metal: valores de <c>CaminoMetal</c> (L.19 / PDF 25).</summary>
    public static readonly IReadOnlySet<string> CaminosMetal = new HashSet<string>
    {
        "brumoso", "nacido-de-la-bruma", "feruquimista", "ferrin", "nacidoble",
    };

    /// <summary>
    /// Caminos alománticos: dan la habilidad Alomancia y un valor de Investidura (L.26 / PDF 32; L.128-129 / PDF 134-135).
    /// </summary>
    public static readonly IReadOnlySet<string> CaminosAlomanticos = new HashSet<string>
    {
        "brumoso", "nacido-de-la-bruma", "nacidoble",
    };

    /// <summary>Caminos feruquímicos: dan la habilidad Feruquimia (L.146, 150, 155 / PDF 152, 156, 161).</summary>
    public static readonly IReadOnlySet<string> CaminosFeruquimicos = new HashSet<string>
    {
        "feruquimista", "ferrin", "nacidoble",
    };

    /// <summary>
    /// Caminos cuyo talento principal exige ascendencia humana (Ruptura de nacido de la bruma, L.141 / PDF 147; Herencia
    /// feruquímica, L.146 / PDF 152). Brumoso, ferrin y nacidoble admiten sangre koloss (L.135, 150, 155 / PDF 141, 156, 161).
    /// </summary>
    public static readonly IReadOnlySet<string> CaminosSoloHumanos = new HashSet<string>
    {
        "nacido-de-la-bruma", "feruquimista",
    };

    /// <summary>Ascendencias (L.32-39 / PDF 38-45), con la misma capitalización que <c>Humano</c>/<c>Oyente</c> de Stormlight.</summary>
    public static readonly IReadOnlySet<string> Ascendencias = new HashSet<string> { Humano, Kandra, SangreKoloss };

    /// <summary>Artes metálicas de un poder de personaje; la hemalurgia no lo es en v1 (L.251 / PDF 257).</summary>
    public static readonly IReadOnlySet<string> Artes = new HashSet<string> { Alomancia, Feruquimia };

    /// <summary>Los 17 metales jugables (ids ASCII, P7), en el orden de las tablas de L.168 / PDF 174 y L.171 / PDF 177.</summary>
    public static readonly IReadOnlyList<string> Metales =
    [
        "hierro", "acero", "estano", "peltre", "cinc", "laton", "cobre", "bronce",
        "cromo", "nicrosil", "aluminio", "duraluminio", "cadmio", "bendaleo", "oro", "electro",
        "atium",
    ];

    /// <summary>
    /// Metales comunes: los 8 físicos y mentales, contenido de un vial estándar (L.130 / PDF 136; L.167 / PDF 173). El resto son raros.
    /// </summary>
    public static readonly IReadOnlySet<string> MetalesComunes = new HashSet<string>
    {
        "hierro", "acero", "estano", "peltre", "cinc", "laton", "cobre", "bronce",
    };

    /// <summary>
    /// Origen de un poder (L.288-295 / PDF 294-301): el camino, un clavo hemalúrgico, una aleación de lerasium o un medallón
    /// feruquímico.
    /// </summary>
    public static readonly IReadOnlySet<string> Origenes = new HashSet<string> { "camino", "clavo", "lerasium", "medallon" };

    /// <summary>Bendición kandra: nombre visible (concepto de su línea de bono) y bonos.</summary>
    public sealed record Bendicion(string Nombre, BonosForma Bonos);

    /// <summary>
    /// Bendiciones kandra (L.34-35 / PDF 40-41): una al crear y una segunda, distinta, como recompensa en rango 3. Aumentan el
    /// valor y el máximo del atributo, así que se aplican como bonos (no cuentan en el presupuesto de puntos de atributo).
    /// </summary>
    public static readonly IReadOnlyDictionary<string, Bendicion> Bendiciones = new Dictionary<string, Bendicion>
    {
        ["consciencia"] = new("Bendición de la Consciencia", new BonosForma(Discernimiento: 2)),
        ["potencia"]    = new("Bendición de la Potencia", new BonosForma(Fuerza: 1, Velocidad: 1)),
        ["presencia"]   = new("Bendición de la Presencia", new BonosForma(Intelecto: 1, Presencia: 1)),
        ["estabilidad"] = new("Bendición de la Estabilidad", new BonosForma(Voluntad: 2)),
        ["fortaleza"]   = new("Bendición de la Fortaleza", new BonosForma(Desvio: 1)),
    };

    /// <summary>
    /// Talentos que aumentan un atributo de forma permanente: Tamaño desmedido, Fuerza +1 (L.39 / PDF 45); Guardián del
    /// conocimiento, Intelecto +2 (L.229 / PDF 235). Ningún otro talento de los árboles del mundo cambia un atributo.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, BonosForma> TalentosConBonoAtributo = new Dictionary<string, BonosForma>
    {
        ["Tamaño desmedido"]          = new BonosForma(Fuerza: 1),
        ["Guardián del conocimiento"] = new BonosForma(Intelecto: 2),
    };

    /// <summary>
    /// Reglas de talento propias del mundo (<c>IWorldRules.ReglasPropias</c>), que <see cref="TalentosReglas.Efectivas"/> combina
    /// con el núcleo Cosmere. Solo las de Scadrial: las listas del núcleo se comparten por referencia y nadie las muta.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, List<ReglaTalento>> Reglas = new Dictionary<string, List<ReglaTalento>>
    {
        // L.38 / PDF 44: «Durante la creación del personaje y cada vez que subes de nivel, tu salud máxima y actual aumentan en 1».
        ["Resistencia koloss"] =
        [
            new ReglaTalento
            {
                Stat = StatAfectada.MaxSalud, Formula = TipoFormula.PorNivel, Valor = 1, Condicion = CondicionRegla.Siempre,
                DescripcionCondicion = "Resistencia koloss: +1 salud por nivel",
            },
        ],
    };

    static MistbornData()
    {
        // El literal del núcleo no gana reglas de Scadrial y Nacidos de la bruma no hereda las de Roshar. Solo en Debug.
        Debug.Assert(!TalentosReglas.Reglas.ContainsKey("Resistencia koloss")
                     && !TalentosReglas.Efectivas(Reglas).Keys.Intersect(TalentosReglas.ClavesRoshar).Any());
    }
}
