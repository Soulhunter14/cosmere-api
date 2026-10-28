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
    /// <summary>Pasos que el director permite cambiar una vez respondidos (ids de <c>Pasos</c>); los demás quedan como se respondieron.</summary>
    public List<string> Cambiables { get; set; } = [];
    /// <summary>
    /// Cuántos personajes de los demás miembros (no PNJ, vivos, creados o en creación) tienen cada legado (id → número); para el director,
    /// todos los personajes. Aviso al elegir el trasfondo («Ya lo ha elegido…»), sin nombres: los personajes en creación son del director.
    /// </summary>
    public Dictionary<string, int> LegadosElegidos { get; set; } = new();
    /// <summary>La sesión de creación de la campaña (la sesión 0 de la pantalla del director).</summary>
    public SesionCreacionResponse Sesion { get; set; } = new();
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
    /// <summary>El registro de respuestas: todas para el director; las de la creación para el jugador.</summary>
    public List<RespuestaResponse> Respuestas { get; set; } = [];
}
