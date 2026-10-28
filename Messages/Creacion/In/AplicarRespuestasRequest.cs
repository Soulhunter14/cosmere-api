namespace Messages.Creacion.In;

/// <summary>
/// «Aplicar a la ficha» a mano (director): las decisiones del guion aún no aplicadas de un personaje, cada una con lo que da (sumado
/// por la pantalla). Cada fila queda marcada como aplicada con lo hecho, así repetir la petición no suma dos veces. Los caminos y
/// metales no se aplican: viaja la recomendación (el más marcado) para guardarla con la creación del personaje.
/// </summary>
public class AplicarRespuestasRequest
{
    public List<FilaAplicarRequest> Filas { get; set; } = [];
    public string CaminoRecomendado { get; set; } = string.Empty;
    public string MetalRecomendado { get; set; } = string.Empty;
}

public class FilaAplicarRequest
{
    /// <summary>Clave de la decisión (<c>decision:…</c>; el servidor antepone el prefijo si falta).</summary>
    public required string Clave { get; set; }
    public EfectosFicha Efectos { get; set; } = new();
}
