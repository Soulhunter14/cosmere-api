using System.Text.Json.Serialization;

namespace Messages.Characters.Out;

public class StatLinea
{
    public string Concepto { get; set; } = string.Empty;
    public double Valor { get; set; }
    public string? DescripcionCondicion { get; set; }
    /// <summary>Línea de bono de atributo de cualquier origen (forma de cantor, Bendición, talento, clavo), marcada por el servidor.</summary>
    public bool EsBono { get; set; }
    /// <summary>Informative line with no numeric effect (hemalurgy «Desorientado al inicio de escena»): the sheet prints only its
    /// concept. Written only when true, so the JSON of every other line (all of Stormlight) does not change</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool SinValor { get; set; }
}

public class StatDesglose
{
    public double Total { get; set; }
    public string? Unidad { get; set; }             // null = número entero, "m" = metros
    public List<StatLinea> Lineas { get; set; } = [];       // activas — suman al total
    public List<StatLinea> Situacional { get; set; } = [];  // visibles pero NO en el total
}
