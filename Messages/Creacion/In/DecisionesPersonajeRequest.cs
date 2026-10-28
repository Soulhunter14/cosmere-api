using System.Text.Json;

namespace Messages.Creacion.In;

/// <summary>
/// Lo que un personaje ha decidido en el guion de la pantalla del director: la lista completa de sus decisiones, que sustituye a la
/// guardada (las que no vienen se quitan; repetir el envío no duplica nada), y la sesión de mesa en que está la pantalla. Con
/// <see cref="Aplicar"/>, mientras el personaje está en creación, cada decisión escribe en la ficha lo que da según llega y lo deshace
/// si cambia o se quita (la sesión de creación). Solo director.
/// </summary>
public class DecisionesPersonajeRequest
{
    public string SesionId { get; set; } = string.Empty;
    public string SesionTitulo { get; set; } = string.Empty;
    /// <summary>Aplicar a la ficha lo que da cada decisión según llega; solo surte efecto mientras el personaje está en creación.</summary>
    public bool Aplicar { get; set; }
    /// <summary>Camino y metal más marcados entre todas las decisiones del personaje (la pantalla los cuenta); vacíos = sin cambio.</summary>
    public string CaminoRecomendado { get; set; } = string.Empty;
    public string MetalRecomendado { get; set; } = string.Empty;
    public List<DecisionRespuestaRequest> Decisiones { get; set; } = [];
}

/// <summary>Una decisión del guion: la pregunta («### Decisión: …»), la opción elegida, lo que la acompaña y lo que da a la ficha.</summary>
public class DecisionRespuestaRequest
{
    /// <summary>La clave de la pantalla (<c>grupo|escena|pregunta</c>); el servidor antepone <c>decision:</c> si no viene.</summary>
    public required string Clave { get; set; }
    public string Pregunta { get; set; } = string.Empty;
    /// <summary>Texto de la opción elegida.</summary>
    public string Respuesta { get; set; } = string.Empty;
    public string TipoRespuesta { get; set; } = "opcion";
    /// <summary>JSON libre: efectos tal como los escribe el guion, prueba y resultado, escena y grupo, fecha de la elección.</summary>
    public JsonElement? Detalle { get; set; }
    /// <summary>Lo que da a la ficha, ya sumado por la pantalla (incluido el premio de una prueba superada); null = nada.</summary>
    public EfectosFicha? Efectos { get; set; }
}
