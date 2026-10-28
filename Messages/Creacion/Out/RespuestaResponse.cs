using System.Text.Json;

namespace Messages.Creacion.Out;

/// <summary>Una pregunta respondida por (o sobre) un personaje: ver <c>CharacterRespuestaEntity</c>.</summary>
public class RespuestaResponse
{
    public long Id { get; set; }
    public long CharacterId { get; set; }
    public required string Clave { get; set; }
    public string TipoPregunta { get; set; } = "paso";
    public string Pregunta { get; set; } = string.Empty;
    public string TipoRespuesta { get; set; } = "texto";
    public string Respuesta { get; set; } = string.Empty;
    /// <summary>Lo que acompaña a la respuesta (objeto JSON; <c>{}</c> sin nada).</summary>
    public JsonElement Detalle { get; set; }
    public string Origen { get; set; } = "creacion";
    public string SesionId { get; set; } = string.Empty;
    public string SesionTitulo { get; set; } = string.Empty;
    public DateTime RespondidaEn { get; set; }
    public DateTime? AplicadaEn { get; set; }
}
