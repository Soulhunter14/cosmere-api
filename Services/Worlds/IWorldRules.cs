using Messages.Worlds;

namespace Services.Worlds;

/// <summary>
/// Reglas de un mundo (ambientación) de campaña. Interfaz mínima de T04 (<c>Id</c> y <c>NormalizarEra</c>); T10 la
/// amplía con el resto de miembros de la especificación (§6.1).
/// </summary>
public interface IWorldRules
{
    /// <summary>Id del mundo (<see cref="WorldIds"/>).</summary>
    string Id { get; }

    /// <summary>
    /// Normaliza la era recibida al crear la campaña. Stormlight no tiene era: devuelve <c>null</c> siempre.
    /// Nacidos de la bruma exige <c>era1</c> o <c>era2</c> y lanza <see cref="ArgumentException"/> (400) si no.
    /// </summary>
    string? NormalizarEra(string? era);
}

public interface IWorldRulesProvider
{
    /// <summary>Reglas del mundo; un id nulo, vacío o desconocido cae a Stormlight.</summary>
    IWorldRules Get(string? worldId);

    /// <summary>Fuente única de los mundos válidos: son las <see cref="IWorldRules"/> registradas.</summary>
    bool Existe(string? worldId);
}

public sealed class WorldRulesProvider(IEnumerable<IWorldRules> mundos) : IWorldRulesProvider
{
    private readonly Dictionary<string, IWorldRules> _porId = mundos.ToDictionary(s => s.Id);

    public IWorldRules Get(string? id) =>
        id is not null && _porId.TryGetValue(id, out var s) ? s : _porId[WorldIds.Stormlight];

    public bool Existe(string? id) => id is not null && _porId.ContainsKey(id);
}
