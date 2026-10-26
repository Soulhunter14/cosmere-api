namespace Messages.Creacion.In;

/// <summary>Ficha nueva de la creación guiada: el director la asigna a un jugador; el nombre lo pondrá el jugador en su paso.</summary>
public class CreacionPersonajeRequest
{
    public long OwnerId { get; set; }
}
