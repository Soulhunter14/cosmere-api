namespace Services.Worlds;

/// <summary>
/// Tabla «Progresión de las artes metálicas» (L.163 / PDF 169): límite, dado y alcance de las artes metálicas según los grados en
/// la habilidad Investida (Alomancia o Feruquimia). Tabla pura y única del servidor; su gemelo TS, solo para la enciclopedia, es
/// <c>src/data/mistborn/progresionArtes.ts</c> (P5: una tabla por lenguaje y nada más). La ficha y el tirador leen los derivados
/// que calcula <see cref="MistbornRules.Derivar"/>.
/// </summary>
public static class ArtesMetalicas
{
    /// <summary>
    /// Fila de la tabla para <paramref name="grados"/>. Con 0 grados: límite 1, dado 1 («sin tirada») y alcance 3 m. Con 6 o más
    /// (solo con hemalurgia u otros efectos especiales) el límite es «igual al rango» (<paramref name="rango"/>).
    /// </summary>
    public static (int Limite, int DadoCaras, int AlcanceMetros) Progresion(int grados, int rango) => grados switch
    {
        <= 0 => (1, 1, 3),
        1 => (1, 4, 6),
        2 => (2, 6, 12),
        3 => (3, 8, 24),
        4 => (4, 10, 48),
        5 => (5, 12, 96),
        _ => (rango, 20, 192),
    };
}
