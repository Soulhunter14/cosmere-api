namespace Messages.Characters.Out;

public class StatLinea
{
    public string Concepto { get; set; } = string.Empty;
    public double Valor { get; set; }
    public string? DescripcionCondicion { get; set; }
}

public class StatDesglose
{
    public double Total { get; set; }
    public string? Unidad { get; set; }             // null = número entero, "m" = metros
    public List<StatLinea> Lineas { get; set; } = [];       // activas — suman al total
    public List<StatLinea> Situacional { get; set; } = [];  // visibles pero NO en el total
}
