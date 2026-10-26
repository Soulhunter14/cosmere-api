namespace Messages.Creacion.Out;

/// <summary>
/// Estado de la creación guiada de personajes de una campaña: los pasos liberados por el director, la lista de pasos del mundo (ids de
/// campo de <c>CreacionGuiada</c>) y los personajes con lo que llevan. El director ve todos los personajes y sus respuestas; un jugador,
/// solo los suyos.
/// </summary>
public class CreacionResponse
{
    /// <summary>Pasos liberados (0 = sin abrir).</summary>
    public int Paso { get; set; }
    /// <summary>Pasos del mundo, en orden; vacía en un mundo sin creación guiada.</summary>
    public List<string> Pasos { get; set; } = [];
    /// <summary>La campaña ya se inició: la creación guiada queda en solo lectura.</summary>
    public bool Iniciada { get; set; }
    public List<CreacionPersonajeResponse> Personajes { get; set; } = [];
}

public class CreacionPersonajeResponse
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public long? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public string Legado { get; set; } = string.Empty;
    public string Ascendencia { get; set; } = string.Empty;
    public List<string> LegadoRespuestas { get; set; } = [];
    /// <summary>Datos de los pasos que no son campos de la ficha (clave → valor; p. ej. «alomancia»).</summary>
    public Dictionary<string, string> CreacionDatos { get; set; } = new();
    /// <summary>«Personaje creado»: la ficha ya está en «Personajes».</summary>
    public bool Completada { get; set; }
    public DateTime UpdatedAt { get; set; }
}
