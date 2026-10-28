using System.Text.Json;
using Infrastructure.Data;
using Messages.Characters.In;
using Messages.Creacion.Out;
using Messages.Database.Entities;
using Services.Campaigns;

namespace Services.Creacion;

/// <summary>
/// El registro genérico de preguntas y respuestas de un personaje (<see cref="CharacterRespuestaEntity"/>): una fila por pregunta,
/// con la pregunta tal como se hizo, la respuesta y de dónde sale. Lo alimentan los pasos de la creación guiada (desde el <c>PUT</c> del
/// personaje, <see cref="RegistrarPasos"/>) y las decisiones del guion de la pantalla del director (<see cref="RespuestasService"/>).
/// La ficha sigue guardando lo vigente en sus campos y en <c>CreacionDatos</c>: las filas son el registro, no el dato de trabajo.
/// </summary>
public static class RespuestasPersonaje
{
    public const string TipoPaso = "paso";
    public const string TipoDecision = "decision";

    public const string OrigenCreacion = "creacion";
    public const string OrigenSesionCreacion = "sesion-creacion";
    public const string OrigenSesion = "sesion";

    public const string RespuestaTexto = "texto";
    public const string RespuestaOpcion = "opcion";
    public const string RespuestaSiNo = "si-no";
    public const string RespuestaEleccion = "eleccion";

    /// <summary>Prefijo de la clave de una decisión del guion: <c>decision:grupo|escena|pregunta</c>.</summary>
    public const string PrefijoDecision = TipoDecision + ":";

    private static readonly JsonElement DetalleVacio = JsonDocument.Parse("{}").RootElement.Clone();

    /// <summary>Tipo de respuesta de un paso cuando el cliente no lo dice: el legado y la ascendencia son elecciones; un dato «si»/«no», un sí o no; el resto, texto.</summary>
    public static string TipoRespuestaDe(string paso, string valor)
    {
        if (paso is CreacionGuiada.Legado or CreacionGuiada.Ascendencia) return RespuestaEleccion;
        if (paso.StartsWith(CreacionGuiada.Dato + ":", StringComparison.Ordinal))
            return valor is "si" or "no" ? RespuestaSiNo : RespuestaEleccion;
        return RespuestaTexto;
    }

    /// <summary>
    /// Registra los pasos de la creación que cambia una petición (<see cref="CreacionGuiada.Cambios"/>, calculados antes de aplicarla):
    /// un valor nuevo crea o actualiza la fila del paso (con la pregunta que manda el cliente o, si no, el id del paso; cambiar la respuesta
    /// deja <c>AplicadaEn</c> a <c>null</c>); un valor vacío quita la fila. <paramref name="filas"/> son las filas de pasos del personaje,
    /// ya cargadas y seguidas por el contexto; se modifican en sitio y el que llama guarda.
    /// </summary>
    public static void RegistrarPasos(CosmereContext db, long characterId, List<CharacterRespuestaEntity> filas,
        IReadOnlyList<(string campo, string valor)> cambios, IReadOnlyList<RespuestaPasoRequest>? preguntas, DateTime ahora)
    {
        foreach (var (campo, valor) in cambios)
        {
            var fila = filas.FirstOrDefault(f => f.Clave == campo);
            if (valor == "")
            {
                if (fila is null) continue;
                db.CharacterRespuestas.Remove(fila);
                filas.Remove(fila);
                continue;
            }
            var pregunta = preguntas?.FirstOrDefault(p => p.Paso == campo);
            if (fila is null)
            {
                fila = new CharacterRespuestaEntity
                {
                    CharacterId = characterId, Clave = campo, TipoPregunta = TipoPaso, Origen = OrigenCreacion, CreatedAt = ahora, RespondidaEn = ahora,
                };
                db.CharacterRespuestas.Add(fila);
                filas.Add(fila);
            }
            if (!string.IsNullOrWhiteSpace(pregunta?.Pregunta)) fila.Pregunta = pregunta.Pregunta.Trim();
            else if (fila.Pregunta == "") fila.Pregunta = campo;
            fila.TipoRespuesta = !string.IsNullOrWhiteSpace(pregunta?.TipoRespuesta) ? pregunta.TipoRespuesta.Trim() : TipoRespuestaDe(campo, valor);
            if (fila.Respuesta != valor)
            {
                fila.Respuesta = valor;
                fila.RespondidaEn = ahora;
                fila.AplicadaEn = null;
            }
        }
    }

    public static RespuestaResponse MapToResponse(CharacterRespuestaEntity r) => new()
    {
        Id = r.Id,
        CharacterId = r.CharacterId,
        Clave = r.Clave,
        TipoPregunta = r.TipoPregunta,
        Pregunta = r.Pregunta,
        TipoRespuesta = r.TipoRespuesta,
        Respuesta = r.Respuesta,
        Detalle = ParseDetalle(r.Detalle),
        Origen = r.Origen,
        SesionId = r.SesionId,
        SesionTitulo = r.SesionTitulo,
        RespondidaEn = r.RespondidaEn,
        AplicadaEn = r.AplicadaEn,
    };

    /// <summary>El JSON guardado como objeto; <c>{}</c> si está vacío o no es JSON válido.</summary>
    public static JsonElement ParseDetalle(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return DetalleVacio;
        try
        {
            using var doc = JsonDocument.Parse(raw);
            return doc.RootElement.ValueKind == JsonValueKind.Object ? doc.RootElement.Clone() : DetalleVacio;
        }
        catch (JsonException)
        {
            return DetalleVacio;
        }
    }

    /// <summary>El detalle de una petición como texto JSON para guardar; <c>{}</c> si no viene o no es un objeto.</summary>
    public static string SerializarDetalle(JsonElement? detalle) =>
        detalle is { ValueKind: JsonValueKind.Object } d ? d.GetRawText() : "{}";
}
