using Messages.Characters;
using Messages.Characters.In;
using Messages.Characters.Out;
using Messages.Database.Entities;
using Messages.Worlds;
using Services.Characters;

namespace Services.Worlds;

/// <summary>
/// Reglas del mundo «Nacidos de la bruma» (Scadrial). <c>Id</c> y <c>NormalizarEra</c> son propios desde T04; el resto de
/// miembros delega provisionalmente en Stormlight, de modo que una campaña <c>mistborn</c> se comporta exactamente como
/// Stormlight hasta que T12 sustituya la delegación por la implementación real (T13: <c>AplicarAccionMesa</c>).
/// </summary>
public sealed class MistbornRules : IWorldRules
{
    private static readonly StormlightRules Respaldo = new(); // hasta T12

    public string Id => WorldIds.Mistborn;

    /// <summary>La era es obligatoria: <c>era1</c> o <c>era2</c> (L.372 / PDF 378); cualquier otro valor da 400.</summary>
    public string? NormalizarEra(string? era) =>
        EraIds.EsValida(era) ? era : throw new ArgumentException($"Invalid Era: '{era}'.");

    public IReadOnlyDictionary<string, List<ReglaTalento>> ReglasPropias => Respaldo.ReglasPropias;

    public IReadOnlyDictionary<string, List<ReglaTalento>> ReglasTalentos => Respaldo.ReglasTalentos;

    public IReadOnlySet<string> RecursosPermitidos => Respaldo.RecursosPermitidos;

    public bool CaminoInvestidoLoCambiaElDirector => Respaldo.CaminoInvestidoLoCambiaElDirector;

    public bool DesvioBonoSeAcumula => Respaldo.DesvioBonoSeAcumula;

    public void ValidarIdentidad(IdentidadPersonaje id) => Respaldo.ValidarIdentidad(id);

    public void RestringirCambiosNoGm(UpdateCharacterRequest request, CharacterEntity character) =>
        Respaldo.RestringirCambiosNoGm(request, character);

    public void AplicarAccionMesa(CharacterEntity c, AccionMesa accion, CharacterResponse estado) =>
        Respaldo.AplicarAccionMesa(c, accion, estado);

    public void AlConcluirMeta(CharacterEntity c, MetaEntity meta) => Respaldo.AlConcluirMeta(c, meta);

    public void AlBorrarMeta(CharacterEntity c, MetaEntity meta) => Respaldo.AlBorrarMeta(c, meta);

    public IEnumerable<string> TalentosImplicitos(CharacterEntity c) => Respaldo.TalentosImplicitos(c);

    public bool TieneInvestidura(CharacterEntity c, IReadOnlyList<string> talentos, IReadOnlyList<PoderPersonaje> poderes) =>
        Respaldo.TieneInvestidura(c, talentos, poderes);

    public BonosForma BonosAtributos(CharacterEntity c, IReadOnlyList<string> talentos, out string? origen) =>
        Respaldo.BonosAtributos(c, talentos, out origen);

    public string EtiquetaBono(string? origen) => Respaldo.EtiquetaBono(origen);

    public Dictionary<string, StatDesglose> Derivar(CharacterEntity c, IReadOnlyList<string> talentos, IReadOnlyList<PoderPersonaje> poderes, BonosForma fb) =>
        Respaldo.Derivar(c, talentos, poderes, fb);
}
