using System.Diagnostics;
using Messages.Characters;
using Messages.Characters.In;
using Messages.Characters.Out;
using Messages.Database.Entities;
using Messages.Worlds;
using Services.Characters;

namespace Services.Worlds;

/// <summary>
/// Reglas del mundo Stormlight (Roshar). Envuelve el motor actual (incluido el WIP de formas de cantor) sin reescribirlo:
/// cero cambio de comportamiento, verificable con la captura de referencia de T02.
/// </summary>
public sealed class StormlightRules : IWorldRules
{
    // Movidos tal cual desde CharacterService (ValidateCaminos): mismos valores, mensajes y orden.
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

    static StormlightRules()
    {
        // Núcleo Cosmere + reglas de Roshar = el literal TalentosReglas.Reglas, clave a clave y en el mismo orden (el orden
        // del diccionario decide el de las líneas del desglose). Solo en Debug.
        Debug.Assert(TalentosReglas.Efectivas(TalentosReglas.ReglasRoshar).Keys.SequenceEqual(TalentosReglas.Reglas.Keys));
    }

    public string Id => WorldIds.Stormlight;

    /// <summary>Stormlight no tiene era.</summary>
    public string? NormalizarEra(string? era) => null;

    public IReadOnlyDictionary<string, List<ReglaTalento>> ReglasPropias => TalentosReglas.ReglasRoshar;

    /// <summary>Mismas claves, mismas listas y mismo orden que <see cref="TalentosReglas.Reglas"/>.</summary>
    public IReadOnlyDictionary<string, List<ReglaTalento>> ReglasTalentos { get; } =
        TalentosReglas.Efectivas(TalentosReglas.ReglasRoshar);

    public IReadOnlySet<string> RecursosPermitidos { get; } = new HashSet<string>();

    public bool CaminoInvestidoLoCambiaElDirector => true;

    public bool DesvioBonoSeAcumula => false;

    public void ValidarIdentidad(IdentidadPersonaje id)
    {
        if (!string.IsNullOrEmpty(id.CaminoHeroico) && !ValidCaminosHeroicos.Contains(id.CaminoHeroico))
            throw new ArgumentException($"Invalid CaminoHeroico: '{id.CaminoHeroico}'.");
        if (!string.IsNullOrEmpty(id.CaminoRadiante) && !ValidCaminosRadiantes.Contains(id.CaminoRadiante))
            throw new ArgumentException($"Invalid CaminoRadiante: '{id.CaminoRadiante}'.");
        if (!string.IsNullOrEmpty(id.Ascendencia) && !ValidAscendencias.Contains(id.Ascendencia))
            throw new ArgumentException($"Invalid Ascendencia: '{id.Ascendencia}'.");

        // Una campaña Stormlight nunca acumula datos de Nacidos de la bruma (decisión (j), §5.3).
        if (!string.IsNullOrEmpty(id.CaminoMetal))
            throw new ArgumentException($"Invalid CaminoMetal: '{id.CaminoMetal}'.");
        if (!string.IsNullOrEmpty(id.CaminoInicial))
            throw new ArgumentException($"Invalid CaminoInicial: '{id.CaminoInicial}'.");
        if (id.Poderes.Count > 0 || id.Bendiciones.Count > 0 || id.Recursos.Count > 0)
            throw new ArgumentException("This world has no metalborn data.");
    }

    /// <summary>El bloqueo actual del <c>PUT</c> de un no-GM, movido tal cual desde <c>CharacterService</c>.</summary>
    public void RestringirCambiosNoGm(UpdateCharacterRequest request, CharacterEntity character) =>
        request.CaminoRadiante = character.CaminoRadiante;

    public void AplicarAccionMesa(CharacterEntity c, AccionMesa accion, CharacterResponse estado) =>
        throw new ArgumentException("Table action not available in this world.");

    public void AlConcluirMeta(CharacterEntity c, MetaEntity meta) { }

    public void AlBorrarMeta(CharacterEntity c, MetaEntity meta) { }

    public IEnumerable<string> TalentosImplicitos(CharacterEntity c) => [];

    public bool TieneInvestidura(CharacterEntity c, IReadOnlyList<string> talentos, IReadOnlyList<PoderPersonaje> poderes) =>
        !string.IsNullOrEmpty(c.CaminoRadiante);

    /// <summary>Bonos de la forma activa de un cantor (WIP de <see cref="FormasCantor"/>); el origen es la forma.</summary>
    public BonosForma BonosAtributos(CharacterEntity c, IReadOnlyList<string> talentos, out string? origen)
    {
        origen = FormasCantor.EsCantor(c.Ascendencia) ? FormasCantor.FormaActiva(talentos) : null;
        return FormasCantor.BonosActivos(c.Ascendencia, talentos);
    }

    /// <summary>Idéntico a <c>CharacterService.ConceptoForma</c>.</summary>
    public string EtiquetaBono(string? origen) => $"Forma: {origen}";

    public Dictionary<string, StatDesglose> Derivar(CharacterEntity c, IReadOnlyList<string> talentos, IReadOnlyList<PoderPersonaje> poderes, BonosForma fb) => new();
}
