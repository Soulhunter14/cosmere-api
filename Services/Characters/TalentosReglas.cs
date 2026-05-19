using Messages.Characters.Out;
using Messages.Database.Entities;

namespace Services.Characters;

// ── Enums ────────────────────────────────────────────────────────────────────

public enum StatAfectada
{
    MaxConcentracion,
    MaxInvestidura,
    MaxSalud,
    DefensaFisica,
    DefensaCognitiva,
    DefensaEspiritual,
    Desvio,
    Movimiento,
}

public enum TipoFormula
{
    Plana,      // valor fijo
    PorRango,   // ceil(level / 5)  × valor
    PorNivel,   // level × valor
}

public enum CondicionRegla
{
    Siempre,             // siempre activa → suma al total
    TieneInvestidura,    // activa si maxInvestiture > 0
    LlevaArmaduraTipo,   // activa si equippedArmor contiene el tipo
    EnCombate,           // activa cuando el toggle "en combate" está ON
    EsPostura,           // requiere activar postura (futuro) → siempre situacional
    EsReaccion,          // se activa como reacción → siempre situacional
}

// ── Regla individual ─────────────────────────────────────────────────────────

public class ReglaTalento
{
    public StatAfectada Stat { get; set; }
    public TipoFormula Formula { get; set; }
    public double Valor { get; set; } = 1;
    public CondicionRegla Condicion { get; set; } = CondicionRegla.Siempre;
    public string? TipoArmadura { get; set; }           // para LlevaArmaduraTipo
    public string? DescripcionCondicion { get; set; }   // texto legible para el jugador
}

// ── Registro de talentos con reglas ──────────────────────────────────────────

public static class TalentosReglas
{
    public static readonly Dictionary<string, List<ReglaTalento>> Reglas = new()
    {
        // ── Caminos Heroicos ─────────────────────────────────────────────────

        ["Compostura"] =
        [
            new() { Stat = StatAfectada.MaxConcentracion, Formula = TipoFormula.PorRango, Condicion = CondicionRegla.Siempre },
        ],

        ["Robusto"] =
        [
            new() { Stat = StatAfectada.MaxSalud, Formula = TipoFormula.PorNivel, Condicion = CondicionRegla.Siempre },
        ],

        ["Serenidad"] =
        [
            new() { Stat = StatAfectada.DefensaCognitiva,  Formula = TipoFormula.Plana, Valor = 2, Condicion = CondicionRegla.Siempre },
            new() { Stat = StatAfectada.DefensaEspiritual, Formula = TipoFormula.Plana, Valor = 2, Condicion = CondicionRegla.Siempre },
        ],

        ["Paso firme"] =
        [
            new() { Stat = StatAfectada.Movimiento, Formula = TipoFormula.Plana, Valor = 3, Condicion = CondicionRegla.Siempre },
        ],

        ["Vestimenta tradicional"] =
        [
            new()
            {
                Stat = StatAfectada.DefensaFisica, Formula = TipoFormula.Plana, Valor = 2,
                Condicion = CondicionRegla.LlevaArmaduraTipo, TipoArmadura = "Presentable",
                DescripcionCondicion = "Mientras lleva armadura Presentable",
            },
            new()
            {
                Stat = StatAfectada.DefensaEspiritual, Formula = TipoFormula.Plana, Valor = 2,
                Condicion = CondicionRegla.LlevaArmaduraTipo, TipoArmadura = "Presentable",
                DescripcionCondicion = "Mientras lleva armadura Presentable",
            },
        ],

        // Enviado / Mentor
        ["Presciencia"] = [],   // +1 reacción — no es un stat numérico, se omite por ahora

        // ── Órdenes Radiantes ────────────────────────────────────────────────

        ["Investido"] =
        [
            new() { Stat = StatAfectada.MaxInvestidura, Formula = TipoFormula.PorRango, Condicion = CondicionRegla.Siempre },
        ],

        // ── Potencias ────────────────────────────────────────────────────────

        ["Movimiento sin fricción"] =
        [
            new()
            {
                Stat = StatAfectada.Movimiento, Formula = TipoFormula.Plana, Valor = 3,
                Condicion = CondicionRegla.TieneInvestidura,
                DescripcionCondicion = "Mientras tiene Investidura",
            },
        ],

        // ── Posturas (situacional — requieren acción para activarse) ─────────

        ["Posición de la enredadera"] =
        [
            new() { Stat = StatAfectada.DefensaFisica,    Formula = TipoFormula.Plana, Valor = 1, Condicion = CondicionRegla.EsPostura, DescripcionCondicion = "Con postura activa (1 acción)" },
            new() { Stat = StatAfectada.DefensaCognitiva, Formula = TipoFormula.Plana, Valor = 1, Condicion = CondicionRegla.EsPostura, DescripcionCondicion = "Con postura activa (1 acción)" },
        ],

        ["Posición de la sangre"] =
        [
            new() { Stat = StatAfectada.DefensaFisica,     Formula = TipoFormula.Plana, Valor = -2, Condicion = CondicionRegla.EsPostura, DescripcionCondicion = "Con postura activa (1 acción)" },
            new() { Stat = StatAfectada.DefensaCognitiva,  Formula = TipoFormula.Plana, Valor = -2, Condicion = CondicionRegla.EsPostura, DescripcionCondicion = "Con postura activa (1 acción)" },
            new() { Stat = StatAfectada.DefensaEspiritual, Formula = TipoFormula.Plana, Valor = -2, Condicion = CondicionRegla.EsPostura, DescripcionCondicion = "Con postura activa (1 acción)" },
        ],

        // ── Reacciones (siempre situacional) ────────────────────────────────

        ["Parada de tensión"] =
        [
            new() { Stat = StatAfectada.DefensaFisica, Formula = TipoFormula.Plana, Valor = 2, Condicion = CondicionRegla.EsReaccion, DescripcionCondicion = "Como reacción a un ataque" },
        ],

        ["Réplica fulminante"] =
        [
            new() { Stat = StatAfectada.Desvio, Formula = TipoFormula.Plana, Valor = 0, Condicion = CondicionRegla.EsReaccion, DescripcionCondicion = "Añade grados de Disciplina como reacción" },
        ],
    };

    // ── Motor de cálculo ─────────────────────────────────────────────────────

    /// <summary>
    /// Construye un StatDesglose para una stat concreta.
    /// baseLineas son las líneas base (sin talentos) que siempre suman al total.
    /// </summary>
    public static StatDesglose Calcular(
        StatAfectada stat,
        List<StatLinea> baseLineas,
        CharacterEntity c,
        ContextoJuego ctx,
        List<string> talentos,
        string? unidad = null)
    {
        var lineas      = new List<StatLinea>(baseLineas);
        var situacional = new List<StatLinea>();

        foreach (var (nombre, reglas) in Reglas)
        {
            if (!talentos.Contains(nombre)) continue;

            foreach (var regla in reglas.Where(r => r.Stat == stat))
            {
                var valor = ComputeValor(regla, c);
                var linea = new StatLinea
                {
                    Concepto             = nombre,
                    Valor                = valor,
                    DescripcionCondicion = regla.DescripcionCondicion,
                };

                if (EsActiva(regla, c, ctx))
                    lineas.Add(linea);
                else
                    situacional.Add(linea);
            }
        }

        return new StatDesglose
        {
            Total      = lineas.Sum(l => l.Valor),
            Unidad     = unidad,
            Lineas     = lineas,
            Situacional = situacional,
        };
    }

    // ── Helpers internos ─────────────────────────────────────────────────────

    private static bool EsActiva(ReglaTalento regla, CharacterEntity c, ContextoJuego ctx) =>
        regla.Condicion switch
        {
            CondicionRegla.Siempre          => true,
            CondicionRegla.TieneInvestidura => !string.IsNullOrEmpty(c.CaminoRadiante),
            CondicionRegla.LlevaArmaduraTipo =>
                !string.IsNullOrEmpty(c.EquippedArmor) &&
                c.EquippedArmor.Contains(regla.TipoArmadura ?? "", StringComparison.OrdinalIgnoreCase),
            CondicionRegla.EnCombate => ctx.EnCombate,
            _                        => false,  // EsPostura, EsReaccion → siempre situacional
        };

    private static double ComputeValor(ReglaTalento regla, CharacterEntity c) =>
        regla.Formula switch
        {
            TipoFormula.Plana    => regla.Valor,
            TipoFormula.PorRango => Math.Ceiling(c.Level / 5.0) * regla.Valor,
            TipoFormula.PorNivel => c.Level * regla.Valor,
            _                    => 0,
        };

    /// <summary>Movimiento base según Velocidad (metros).</summary>
    public static double MovimientoBase(int velocidad) => velocidad switch
    {
        0    => 6.0,
        <= 2 => 7.5,
        <= 4 => 9.0,
        <= 6 => 12.0,
        <= 8 => 18.0,
        _    => 24.0,
    };
}
