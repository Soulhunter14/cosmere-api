namespace Messages.Worlds;

/// <summary>
/// Eras de una campaña de «Nacidos de la bruma» (L.372 / PDF 378). Obligatoria en <c>mistborn</c> y <c>null</c> en
/// Stormlight; se fija al crear la campaña y es inmutable. No existe un valor «entre eras».
/// </summary>
public static class EraIds
{
    /// <summary>«Era 1: El Mundo de Ceniza».</summary>
    public const string Era1 = "era1";

    /// <summary>«Era 2: Cambio y revolución».</summary>
    public const string Era2 = "era2";

    public static readonly string[] Todas = [Era1, Era2];

    public static bool EsValida(string? v) => v is not null && Todas.Contains(v);

    /// <summary>
    /// Número con el que el catálogo guarda la era de un objeto (columna <c>Era</c> smallint: 1 o 2, <c>null</c> = ambas):
    /// <c>era1</c> → 1, <c>era2</c> → 2; <c>null</c> si no hay era (Stormlight) o el valor no es una era.
    /// </summary>
    public static short? Numero(string? era) => era switch { Era1 => 1, Era2 => 2, _ => null };
}
