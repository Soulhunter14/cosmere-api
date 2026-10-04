namespace Services.Characters;

/// <summary>
/// Bonos numéricos de la forma activa de un cantor (Manual de juego, cap. 2, «Formas», pp. 32–37 del libro).
/// Espejo de <c>bonusAtributos</c> en <c>cosmere-web/src/data/cantores.ts</c>: si cambia uno, cambia el otro.
/// </summary>
public record BonosForma(
    int Fuerza = 0,
    int Velocidad = 0,
    int Intelecto = 0,
    int Voluntad = 0,
    int Discernimiento = 0,
    int Presencia = 0,
    int Desvio = 0,
    int Concentracion = 0)
{
    public static readonly BonosForma Ninguno = new();
    public bool EsVacio => Fuerza == 0 && Velocidad == 0 && Intelecto == 0 && Voluntad == 0
                           && Discernimiento == 0 && Presencia == 0 && Desvio == 0 && Concentracion == 0;
}

public static class FormasCantor
{
    /// <summary>La forma activa se guarda dentro del JSON de talentos como «~forma~Nombre».</summary>
    public const string Prefijo = "~forma~";

    private static readonly HashSet<string> AscendenciasCantor =
        new(StringComparer.OrdinalIgnoreCase) { "Oyente", "Cantor", "Cantora" };

    public static readonly Dictionary<string, BonosForma> Bonos = new()
    {
        // Formas iniciales (sin bonos)
        ["Forma gris"]         = BonosForma.Ninguno,
        ["Forma carnal"]       = BonosForma.Ninguno,
        // Formas de delicadeza
        ["Forma artística"]    = new(Discernimiento: 1),
        ["Forma diestra"]      = new(Velocidad: 1, Concentracion: 2),
        // Formas de determinación
        ["Forma de guerra"]    = new(Fuerza: 1, Desvio: 1),
        ["Forma de trabajo"]   = new(Voluntad: 1),
        // Formas de sabiduría
        ["Forma de mediación"] = new(Presencia: 1),
        ["Forma sabia"]        = new(Intelecto: 1),
        // Formas de destrucción (vacíospren)
        ["Forma funesta"]      = new(Fuerza: 2, Desvio: 2),
        ["Forma tormenta"]     = new(Fuerza: 1, Velocidad: 1, Desvio: 1),
        // Formas de expansión (vacíospren)
        ["Forma emisaria"]     = new(Intelecto: 1, Presencia: 1),
        ["Forma comunicadora"] = new(Velocidad: 2),
        // Formas de misterio (vacíospren)
        ["Forma pútrida"]      = new(Voluntad: 2),
        ["Forma nocturna"]     = new(Discernimiento: 1, Intelecto: 1, Concentracion: 2),
    };

    public static bool EsCantor(string? ascendencia) =>
        !string.IsNullOrEmpty(ascendencia) && AscendenciasCantor.Contains(ascendencia);

    /// <summary>Nombre de la forma activa guardada en la lista de talentos, o null.</summary>
    public static string? FormaActiva(IEnumerable<string> talentos) =>
        talentos.FirstOrDefault(t => t.StartsWith(Prefijo, StringComparison.Ordinal))?[Prefijo.Length..];

    /// <summary>Bonos de la forma activa. Solo aplica a cantores; una forma desconocida no da bonos.</summary>
    public static BonosForma BonosActivos(string? ascendencia, IEnumerable<string> talentos)
    {
        if (!EsCantor(ascendencia)) return BonosForma.Ninguno;
        var forma = FormaActiva(talentos);
        return forma is not null && Bonos.TryGetValue(forma, out var b) ? b : BonosForma.Ninguno;
    }
}
