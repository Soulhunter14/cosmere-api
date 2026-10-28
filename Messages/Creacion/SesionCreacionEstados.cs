namespace Messages.Creacion;

/// <summary>
/// Estados de la sesión de creación de una campaña (<c>Campaign.SesionCreacionEstado</c>): la sesión 0 de la pantalla del director, que se
/// juega una sola vez. <c>pendiente</c> → <c>preparando</c> (el director la prepara en la pantalla) → <c>en-curso</c> (se está jugando: lo que
/// la pantalla guarda en los personajes es de la creación) → <c>jugada</c>. No hay vuelta atrás desde la interfaz.
/// </summary>
public static class SesionCreacionEstados
{
    public const string Pendiente = "pendiente";
    public const string Preparando = "preparando";
    public const string EnCurso = "en-curso";
    public const string Jugada = "jugada";
}
