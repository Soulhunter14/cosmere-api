namespace Messages.Creacion.In;

/// <summary>
/// Al iniciar la sesión de creación desde la pantalla del director: la sesión de mesa que la pantalla acaba de abrir (su id) y el título
/// del guion cargado, para la trazabilidad de las decisiones. Los dos pueden venir vacíos.
/// </summary>
public class SesionCreacionRequest
{
    public string MesaId { get; set; } = string.Empty;
    public string Guion { get; set; } = string.Empty;
}
