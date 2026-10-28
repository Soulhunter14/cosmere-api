namespace Messages.Database.Entities;

/// <summary>
/// Una pregunta respondida por (o sobre) un personaje, con su respuesta y de dónde sale: el registro genérico de la creación guiada y de
/// las decisiones de la mesa (pantalla del director). Una fila por pregunta y personaje (<c>Clave</c> única por personaje): la ficha
/// guarda lo vigente en sus campos; aquí queda lo que se preguntó y lo que se respondió, para leerlo después (el director en «Ver
/// respuestas») y, más adelante, aplicarlo a la ficha una sola vez (<c>AplicadaEn</c>). Ver <c>RespuestasPersonaje</c>.
/// </summary>
public class CharacterRespuestaEntity
{
    public long Id { get; set; }
    public long CharacterId { get; set; }
    /// <summary>
    /// Identifica la pregunta; única por personaje. Un paso de la creación guiada (<c>name</c>, <c>legadoRespuestas:0</c>,
    /// <c>creacion:alomante</c>) o una decisión del guion de la pantalla (<c>decision:grupo|escena|pregunta</c>).
    /// </summary>
    public required string Clave { get; set; }
    /// <summary>Qué clase de pregunta: <c>paso</c> (creación guiada), <c>decision</c> (un «### Decisión» del guion)… Texto abierto: se amplía sin migración.</summary>
    public string TipoPregunta { get; set; } = "paso";
    /// <summary>El texto de la pregunta tal como se hizo en ese momento, aunque luego se reescriba.</summary>
    public string Pregunta { get; set; } = string.Empty;
    /// <summary><c>texto</c> libre, <c>opcion</c> de una lista, <c>si-no</c> o <c>eleccion</c> de un catálogo (legado, metal).</summary>
    public string TipoRespuesta { get; set; } = "texto";
    /// <summary>El texto escrito, la opción elegida, «si»/«no» o el id elegido.</summary>
    public string Respuesta { get; set; } = string.Empty;
    /// <summary>JSON libre con lo que acompaña a la respuesta: en una decisión, sus efectos y la prueba con su resultado. <c>{}</c> sin nada.</summary>
    public string Detalle { get; set; } = "{}";
    /// <summary><c>creacion</c> (página de creación), <c>sesion-creacion</c> (pantalla, con la sesión de creación en curso) o <c>sesion</c> (pantalla, después).</summary>
    public string Origen { get; set; } = "creacion";
    /// <summary>Sesión de mesa de la pantalla en que se respondió (id y título); vacíos desde la página de creación.</summary>
    public string SesionId { get; set; } = string.Empty;
    public string SesionTitulo { get; set; } = string.Empty;
    public DateTime RespondidaEn { get; set; } = DateTime.UtcNow;
    /// <summary>Cuándo se aplicó a la ficha lo que da la respuesta; <c>null</c> = aún no (vuelve a <c>null</c> si la respuesta cambia o se deshace).</summary>
    public DateTime? AplicadaEn { get; set; }
    /// <summary>JSON con lo pedido y lo hecho al aplicarla (<c>AplicadorFicha.RegistroAplicado</c>), para deshacerlo exactamente; <c>{}</c> sin aplicar.</summary>
    public string Aplicado { get; set; } = "{}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CharacterEntity Character { get; set; } = null!;
}
