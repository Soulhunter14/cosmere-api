namespace Messages.Creacion.Out;

/// <summary>Resultado de «Aplicar a la ficha»: el registro de respuestas del personaje al día, lo que se escribió en la ficha y lo que no se pudo colocar.</summary>
public class AplicarRespuestasResponse
{
    public List<RespuestaResponse> Respuestas { get; set; } = [];
    /// <summary>Líneas legibles de lo aplicado («Fuerza 1 → 2», «Atletismo 0 → 1», «Pericia Bajos fondos», «Meta «…»», «Objeto …»).</summary>
    public List<string> Aplicado { get; set; } = [];
    /// <summary>Lo que no encontró sitio en la ficha (una habilidad desconocida, un tope ya alcanzado): para anotarlo a mano.</summary>
    public List<string> Ignorado { get; set; } = [];
}
