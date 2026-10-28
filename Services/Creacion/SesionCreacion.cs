using Messages.Creacion;
using Messages.Creacion.In;
using Messages.Creacion.Out;
using Messages.Database.Entities;

namespace Services.Creacion;

/// <summary>
/// La sesión de creación de una campaña (la sesión 0 de la pantalla del director, que se juega una sola vez), como estado de la campaña:
/// <c>pendiente</c> → <c>preparando</c> → <c>en-curso</c> → <c>jugada</c> (<see cref="SesionCreacionEstados"/>). Mientras está en curso, las
/// decisiones que la pantalla guarda en los personajes nacen con origen <c>sesion-creacion</c> (<see cref="RespuestasPersonaje"/>). Un paso
/// que no toca (terminar sin haber empezado, empezar dos veces) es un conflicto (409). La única vuelta atrás es <c>volver</c> (en curso →
/// preparando), para un inicio por error: una sesión jugada no se reabre.
/// </summary>
public static class SesionCreacion
{
    public const string Preparar = "preparar";
    public const string Iniciar = "iniciar";
    public const string Terminar = "terminar";
    public const string Volver = "volver";

    /// <summary>Aplica una acción del director sobre la campaña. <see cref="InvalidOperationException"/> (409) si el estado no la admite.</summary>
    public static void Aplicar(CampaignEntity c, string accion, SesionCreacionRequest? r, DateTime ahora)
    {
        switch (accion)
        {
            case Preparar:
                Exigir(c, accion, SesionCreacionEstados.Pendiente);
                c.SesionCreacionEstado = SesionCreacionEstados.Preparando;
                c.SesionCreacionPreparadaEn = ahora;
                break;
            case Iniciar:
                Exigir(c, accion, SesionCreacionEstados.Pendiente, SesionCreacionEstados.Preparando);
                c.SesionCreacionEstado = SesionCreacionEstados.EnCurso;
                c.SesionCreacionPreparadaEn ??= ahora;
                c.SesionCreacionIniciadaEn = ahora;
                c.SesionCreacionMesaId = (r?.MesaId ?? "").Trim();
                c.SesionCreacionGuion = (r?.Guion ?? "").Trim();
                break;
            case Terminar:
                Exigir(c, accion, SesionCreacionEstados.EnCurso);
                c.SesionCreacionEstado = SesionCreacionEstados.Jugada;
                c.SesionCreacionTerminadaEn = ahora;
                break;
            case Volver:
                Exigir(c, accion, SesionCreacionEstados.EnCurso);
                c.SesionCreacionEstado = SesionCreacionEstados.Preparando;
                c.SesionCreacionIniciadaEn = null;
                c.SesionCreacionMesaId = string.Empty;
                c.SesionCreacionGuion = string.Empty;
                break;
            default:
                throw new ArgumentException($"Acción desconocida de la sesión de creación: '{accion}'.");
        }
    }

    public static SesionCreacionResponse Respuesta(CampaignEntity c) => new()
    {
        Estado = c.SesionCreacionEstado,
        PreparadaEn = c.SesionCreacionPreparadaEn,
        IniciadaEn = c.SesionCreacionIniciadaEn,
        TerminadaEn = c.SesionCreacionTerminadaEn,
        Guion = c.SesionCreacionGuion,
        MesaId = c.SesionCreacionMesaId,
    };

    private static void Exigir(CampaignEntity c, string accion, params string[] admitidos)
    {
        if (!admitidos.Contains(c.SesionCreacionEstado))
            throw new InvalidOperationException($"No se puede {accion} la sesión de creación: está «{c.SesionCreacionEstado}».");
    }
}
