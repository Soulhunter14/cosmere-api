namespace Messages.Creacion.Out;

/// <summary>La sesión de creación de la campaña (ver <c>SesionCreacionEstados</c>): su estado, sus fechas, el guion y la sesión de mesa con que se jugó.</summary>
public class SesionCreacionResponse
{
    public string Estado { get; set; } = SesionCreacionEstados.Pendiente;
    public DateTime? PreparadaEn { get; set; }
    public DateTime? IniciadaEn { get; set; }
    public DateTime? TerminadaEn { get; set; }
    public string Guion { get; set; } = string.Empty;
    public string MesaId { get; set; } = string.Empty;
}
