using Messages.Creacion.In;
using Messages.Creacion.Out;

namespace Services.Creacion;

/// <summary>El registro de respuestas de un personaje (ver <see cref="RespuestasPersonaje"/>): lectura, y las decisiones del guion que manda la pantalla del director.</summary>
public interface IRespuestasService
{
    /// <summary>Las respuestas del personaje: todas para el director; las de la creación (<c>origen = creacion</c>) para su dueño.</summary>
    Task<List<RespuestaResponse>> GetAsync(long campaignId, long characterId, long userId);

    /// <summary>
    /// Sustituye las decisiones del guion del personaje por las de la petición (director): las que no vienen se quitan, las que cambian
    /// de opción vuelven a quedar sin aplicar. Con la sesión de creación en curso nacen con origen <c>sesion-creacion</c>.
    /// </summary>
    Task<List<RespuestaResponse>> PutDecisionesAsync(long campaignId, long characterId, DecisionesPersonajeRequest request, long userId);

    /// <summary>
    /// «Aplicar a la ficha» (director): escribe en la ficha lo que dan las decisiones de las claves pedidas que aún no estaban aplicadas
    /// (<c>AplicadorFicha</c>), crea sus metas y las marca como aplicadas; 409 si ninguna estaba pendiente.
    /// </summary>
    Task<AplicarRespuestasResponse> AplicarAsync(long campaignId, long characterId, AplicarRespuestasRequest request, long userId);
}
