using Messages.Worlds;

namespace Services.Worlds;

/// <summary>Reglas del mundo «Nacidos de la bruma» (Scadrial). Versión mínima de T04; T10 y T12 la completan.</summary>
public sealed class MistbornRules : IWorldRules
{
    public string Id => WorldIds.Mistborn;

    /// <summary>La era es obligatoria: <c>era1</c> o <c>era2</c> (L.372 / PDF 378); cualquier otro valor da 400.</summary>
    public string? NormalizarEra(string? era) =>
        EraIds.EsValida(era) ? era : throw new ArgumentException($"Invalid Era: '{era}'.");
}
