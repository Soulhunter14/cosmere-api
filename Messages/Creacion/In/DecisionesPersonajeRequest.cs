using System.Text.Json;

namespace Messages.Creacion.In;

/// <summary>
/// Lo que un personaje ha decidido en el guion de la pantalla del director: la lista completa de sus decisiones, que sustituye a la
/// guardada (las que no vienen se quitan; repetir el envío no duplica nada), y la sesión de mesa en que está la pantalla. Solo director.
/// </summary>
public class DecisionesPersonajeRequest
{
    public string SesionId { get; set; } = string.Empty;
    public string SesionTitulo { get; set; } = string.Empty;
    public List<DecisionRespuestaRequest> Decisiones { get; set; } = [];
}

/// <summary>Una decisión del guion: la pregunta («### Decisión: …»), la opción elegida y lo que la acompaña (efectos, prueba, escena…).</summary>
public class DecisionRespuestaRequest
{
    /// <summary>La clave de la pantalla (<c>grupo|escena|pregunta</c>); el servidor antepone <c>decision:</c> si no viene.</summary>
    public required string Clave { get; set; }
    public string Pregunta { get; set; } = string.Empty;
    /// <summary>Texto de la opción elegida.</summary>
    public string Respuesta { get; set; } = string.Empty;
    public string TipoRespuesta { get; set; } = "opcion";
    /// <summary>JSON libre: efectos, prueba y resultado, escena y grupo, fecha de la elección.</summary>
    public JsonElement? Detalle { get; set; }
}
