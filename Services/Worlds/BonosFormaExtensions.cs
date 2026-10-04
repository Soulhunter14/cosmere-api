using Services.Characters;

namespace Services.Worlds;

/// <summary>
/// Extensiones de <see cref="BonosForma"/> (record del WIP de formas de cantor, que no se edita): representa bonos de atributo de
/// cualquier origen (forma de cantor, Bendición, talento, clavo).
/// </summary>
internal static class BonosFormaExtensions
{
    /// <summary>
    /// Bonos de los seis atributos para <c>CharacterResponse.BonosAtributos</c>, sin ceros (claves <c>fuerza</c> …
    /// <c>presencia</c>). <c>Desvio</c> y <c>Concentracion</c> no se emiten: ya viajan como líneas de desglose.
    /// </summary>
    public static Dictionary<string, int> ComoDiccionario(this BonosForma fb)
    {
        var d = new Dictionary<string, int>();
        void Add(string k, int v) { if (v != 0) d[k] = v; }
        Add("fuerza", fb.Fuerza);
        Add("velocidad", fb.Velocidad);
        Add("intelecto", fb.Intelecto);
        Add("voluntad", fb.Voluntad);
        Add("discernimiento", fb.Discernimiento);
        Add("presencia", fb.Presencia);
        return d;
    }
}
