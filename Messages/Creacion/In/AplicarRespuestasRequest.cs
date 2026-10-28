namespace Messages.Creacion.In;

/// <summary>
/// «Aplicar a la ficha» (director): lo que dan las decisiones del guion aún no aplicadas de un personaje, ya sumado por la pantalla, y las
/// claves de esas decisiones, que quedan marcadas como aplicadas en la misma petición (repetirla no suma dos veces). Los caminos y
/// metales no se aplican: viaja la recomendación (el más marcado) para guardarla con la creación del personaje.
/// </summary>
public class AplicarRespuestasRequest
{
    /// <summary>Claves de las decisiones que se aplican (<c>decision:…</c>; el servidor antepone el prefijo si falta).</summary>
    public List<string> Claves { get; set; } = [];
    /// <summary>Atributo (<c>fuerza</c> … <c>presencia</c>) → puntos que suma.</summary>
    public Dictionary<string, int> Atributos { get; set; } = new();
    /// <summary>Habilidad (campo de la ficha: <c>atletismo</c>, <c>armasLigeras</c>…, o el nombre de una habilidad personalizada) → grados que suma.</summary>
    public Dictionary<string, int> Habilidades { get; set; } = new();
    public List<string> Pericias { get; set; } = [];
    /// <summary>Títulos de las metas que nacen activas (una vez por título).</summary>
    public List<string> Metas { get; set; } = [];
    /// <summary>Objetos que entran en el equipo (sin repetir).</summary>
    public List<string> Objetos { get; set; } = [];
    public string CaminoRecomendado { get; set; } = string.Empty;
    public string MetalRecomendado { get; set; } = string.Empty;
}
