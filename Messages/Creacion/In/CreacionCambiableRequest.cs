namespace Messages.Creacion.In;

/// <summary>Permitir o denegar que los jugadores cambien un paso de la creación guiada una vez respondido (<c>Paso</c>: id de campo, p. ej. «legado»).</summary>
public class CreacionCambiableRequest
{
    public required string Paso { get; set; }
    public bool Permitido { get; set; }
}
