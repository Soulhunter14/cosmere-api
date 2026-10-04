using Messages.Worlds;

namespace Services.Worlds;

/// <summary>Reglas del mundo Stormlight (Roshar). Versión mínima de T04; T10 envuelve aquí el motor actual.</summary>
public sealed class StormlightRules : IWorldRules
{
    public string Id => WorldIds.Stormlight;

    /// <summary>Stormlight no tiene era.</summary>
    public string? NormalizarEra(string? era) => null;
}
