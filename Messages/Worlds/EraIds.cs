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
}
