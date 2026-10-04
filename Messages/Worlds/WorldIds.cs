namespace Messages.Worlds;

/// <summary>
/// Identificadores de mundo (ambientación) de una campaña. Los ids son los nombres de los manuales y no cambian.
/// Cada campaña tiene exactamente un mundo, fijado al crearla e inmutable.
/// </summary>
public static class WorldIds
{
    /// <summary>Roshar: manual «Archivo de las Tormentas».</summary>
    public const string Stormlight = "stormlight";

    /// <summary>Scadrial: manual «Nacidos de la bruma».</summary>
    public const string Mistborn = "mistborn";

    /// <summary>
    /// Contenido compartido por todos los mundos (CatalogOptions, M3). No es un mundo de campaña y no entra en
    /// <see cref="Todos"/>: <c>EsValido("cosmere")</c> es <c>false</c>.
    /// </summary>
    public const string Cosmere = "cosmere";

    public static readonly string[] Todos = [Stormlight, Mistborn];

    public static bool EsValido(string? v) => v is not null && Todos.Contains(v);
}
