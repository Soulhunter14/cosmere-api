namespace Messages.Characters.In;

/// <summary>
/// La pregunta de un paso de la creación guiada tal como la vio el jugador, para el registro de respuestas (<c>RespuestasPersonaje</c>):
/// viaja en el <c>PUT</c> del personaje junto al paso que responde. Sin ella, el registro guarda el id del paso como pregunta.
/// </summary>
public class RespuestaPasoRequest
{
    /// <summary>Id del paso (<c>legadoRespuestas:0</c>, <c>creacion:alomante</c>…).</summary>
    public required string Paso { get; set; }
    public string Pregunta { get; set; } = string.Empty;
    /// <summary><c>texto</c>, <c>opcion</c>, <c>si-no</c> o <c>eleccion</c>; vacío = según el paso.</summary>
    public string TipoRespuesta { get; set; } = string.Empty;
}
