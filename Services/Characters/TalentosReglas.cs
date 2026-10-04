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
    Plana,          // valor fijo
    PorRango,       // rango (1–5) × valor
    PorNivel,       // level × valor
    PorHabilidad,   // grados en la habilidad indicada en Habilidad × valor
}

public enum CondicionRegla
{
    Siempre,             // siempre activa → suma al total
    TieneInvestidura,    // activa si el personaje tiene camino Radiante
    LlevaArmaduraTipo,   // activa si equippedArmor contiene el tipo
    EnCombate,           // activa cuando el toggle "en combate" está ON
    EsPostura,           // requiere activar postura (futuro) → siempre situacional
    EsReaccion,          // se activa como reacción → siempre situacional
    InfusoAbrasion,      // solo mientras está infundido con Abrasión (gasta Investidura) → siempre situacional
}

// ── Regla individual ─────────────────────────────────────────────────────────

public class ReglaTalento
{
    public StatAfectada Stat { get; set; }
    public TipoFormula Formula { get; set; }
    public double Valor { get; set; } = 1;
    public CondicionRegla Condicion { get; set; } = CondicionRegla.Siempre;
    public string? TipoArmadura { get; set; }           // para LlevaArmaduraTipo
    public string? Habilidad { get; set; }              // para PorHabilidad (p. ej. "Disciplina")
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

        // ── Cantores ─────────────────────────────────────────────────────────

        // Manual p. 35 (libro): «Aumenta tu Defensa cognitiva en 2».
        ["Mente ambiciosa"] =
        [
            new() { Stat = StatAfectada.DefensaCognitiva, Formula = TipoFormula.Plana, Valor = 2, Condicion = CondicionRegla.Siempre },
        ],

        // ── Órdenes Radiantes ────────────────────────────────────────────────

        ["Investido"] =
        [
            new() { Stat = StatAfectada.MaxInvestidura, Formula = TipoFormula.PorRango, Condicion = CondicionRegla.Siempre },
        ],

        // ── Potencias ────────────────────────────────────────────────────────

        // Manual p. 214 (libro): «Mientras estás infundido con Abrasión, tu valor de movimiento aumenta en 3 metros».
        // Es un efecto temporal que cuesta Investidura, así que nunca suma al total: siempre situacional.
        ["Movimiento sin fricción"] =
        [
            new()
            {
                Stat = StatAfectada.Movimiento, Formula = TipoFormula.Plana, Valor = 3,
                Condicion = CondicionRegla.InfusoAbrasion,
                DescripcionCondicion = "Mientras estás infundido con Abrasión",
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

        // Manual p. 95 (libro): al usar Desafío inalterable como reacción, «aumenta también tu valor de desvío
        // contra este ataque en la misma cantidad que tus grados en Disciplina».
        ["Réplica fulminante"] =
        [
            new()
            {
                Stat = StatAfectada.Desvio, Formula = TipoFormula.PorHabilidad, Habilidad = "Disciplina",
                Condicion = CondicionRegla.EsReaccion,
                DescripcionCondicion = "Contra el ataque al que reaccionas con Desafío inalterable (grados en Disciplina)",
            },
        ],
    };

    // ── Motor de cálculo ─────────────────────────────────────────────────────

    /// <summary>
    /// Construye un StatDesglose para una stat concreta.
    /// baseLineas son las líneas base (sin talentos) que siempre suman al total.
    /// </summary>
    /// <param name="reglas">Reglas de talento efectivas del mundo de la campaña (<c>IWorldRules.ReglasTalentos</c>); su orden decide el de las líneas.</param>
    /// <param name="tieneInvestidura">Si el personaje tiene Investidura según su mundo (<c>IWorldRules.TieneInvestidura</c>); decide <see cref="CondicionRegla.TieneInvestidura"/>.</param>
    public static StatDesglose Calcular(
        StatAfectada stat,
        List<StatLinea> baseLineas,
        CharacterEntity c,
        ContextoJuego ctx,
        List<string> talentos,
        IReadOnlyDictionary<string, List<ReglaTalento>> reglas,
        bool tieneInvestidura,
        string? unidad = null,
        List<StatLinea>? situacionalBase = null)
    {
        var lineas      = new List<StatLinea>(baseLineas);
        var situacional = new List<StatLinea>(situacionalBase ?? []);

        foreach (var (nombre, rs) in reglas)
        {
            if (!talentos.Contains(nombre)) continue;

            foreach (var regla in rs.Where(r => r.Stat == stat))
            {
                var valor = ComputeValor(regla, c);
                var linea = new StatLinea
                {
                    Concepto             = nombre,
                    Valor                = valor,
                    DescripcionCondicion = regla.DescripcionCondicion,
                };

                if (EsActiva(regla, c, ctx, tieneInvestidura))
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

    private static bool EsActiva(ReglaTalento regla, CharacterEntity c, ContextoJuego ctx, bool tieneInvestidura) =>
        regla.Condicion switch
        {
            CondicionRegla.Siempre          => true,
            CondicionRegla.TieneInvestidura => tieneInvestidura,
            CondicionRegla.LlevaArmaduraTipo =>
                !string.IsNullOrEmpty(c.EquippedArmor) &&
                c.EquippedArmor.Contains(regla.TipoArmadura ?? "", StringComparison.OrdinalIgnoreCase),
            CondicionRegla.EnCombate => ctx.EnCombate,
            _                        => false,  // EsPostura, EsReaccion, InfusoAbrasion → siempre situacional
        };

    private static double ComputeValor(ReglaTalento regla, CharacterEntity c) =>
        regla.Formula switch
        {
            TipoFormula.Plana        => regla.Valor,
            TipoFormula.PorRango     => Rango(c.Level) * regla.Valor,
            TipoFormula.PorNivel     => c.Level * regla.Valor,
            TipoFormula.PorHabilidad => GradosDe(c, regla.Habilidad) * regla.Valor,
            _                        => 0,
        };

    /// <summary>Rango de juego (Manual p. 24 del libro): niveles 1–5 → 1 … 16–20 → 4, y 21 o más → 5.</summary>
    public static int Rango(int level) => Math.Clamp((int)Math.Ceiling(level / 5.0), 1, 5);

    /// <summary>Grados del personaje en una habilidad, por su nombre tal y como lo usa el libro.</summary>
    private static int GradosDe(CharacterEntity c, string? habilidad) => habilidad switch
    {
        "Agilidad"         => c.Agilidad,
        "Armamento ligero" => c.ArmasLigeras,
        "Armamento pesado" => c.ArmasPesadas,
        "Atletismo"        => c.Atletismo,
        "Hurto"            => c.Hurto,
        "Sigilo"           => c.Sigilo,
        "Deducción"        => c.Deduccion,
        "Disciplina"       => c.Disciplina,
        "Intimidación"     => c.Intimidacion,
        "Manufactura"      => c.Manufactura,
        "Medicina"         => c.Medicina,
        "Saber"            => c.Conocimiento,
        "Engaño"           => c.Engano,
        "Liderazgo"        => c.Liderazgo,
        "Percepción"       => c.Percepcion,
        "Perspicacia"      => c.Perspicacia,
        "Persuasión"       => c.Persuasion,
        "Supervivencia"    => c.Supervivencia,
        // Habilidades personalizadas (p. ej. las Investidas Alomancia y Feruquimia), por el nombre de su hueco.
        _                  => GradosHabilidadPersonalizada(c, habilidad) ?? 0,
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

    // ── Núcleo Cosmere y reglas de Roshar ────────────────────────────────────
    // El literal Reglas no se edita: sigue siendo la unión de las dos partes. Lo compartido por todos los mundos es
    // ReglasCosmere; cada mundo aporta sus reglas propias (IWorldRules.ReglasPropias) y usa Efectivas(propias).
    // Las listas se comparten por referencia y nadie las muta.

    /// <summary>
    /// Claves de <see cref="Reglas"/> propias de Roshar (cantores, potencias, posturas y reacciones). «Investido» es Cosmere.
    /// </summary>
    public static readonly IReadOnlySet<string> ClavesRoshar = new HashSet<string>
    {
        "Mente ambiciosa", "Movimiento sin fricción", "Posición de la enredadera", "Posición de la sangre",
        "Parada de tensión", "Réplica fulminante",
    };

    /// <summary>Reglas de Roshar: las entradas de <see cref="Reglas"/> cuyas claves están en <see cref="ClavesRoshar"/>.</summary>
    public static readonly IReadOnlyDictionary<string, List<ReglaTalento>> ReglasRoshar =
        Reglas.Where(kv => ClavesRoshar.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);

    /// <summary>
    /// Núcleo Cosmere: el resto de <see cref="Reglas"/> (Compostura, Robusto, Serenidad, Paso firme, Vestimenta tradicional,
    /// Presciencia, Investido).
    /// </summary>
    public static readonly IReadOnlyDictionary<string, List<ReglaTalento>> ReglasCosmere =
        Reglas.Where(kv => !ClavesRoshar.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);

    /// <summary>
    /// Reglas efectivas de un mundo: recorre <see cref="Reglas"/> en el orden del literal y toma cada clave de las reglas
    /// propias del mundo o, si no la redefine, del núcleo Cosmere; después añade, en su orden, las propias que no están en el
    /// literal. El orden del diccionario decide el de las líneas del desglose: <c>Efectivas(ReglasRoshar)</c> reproduce
    /// <see cref="Reglas"/> clave a clave, con las mismas listas.
    /// </summary>
    public static IReadOnlyDictionary<string, List<ReglaTalento>> Efectivas(IReadOnlyDictionary<string, List<ReglaTalento>> propias)
    {
        var efectivas = new Dictionary<string, List<ReglaTalento>>();
        foreach (var nombre in Reglas.Keys)
        {
            if (propias.TryGetValue(nombre, out var propia)) efectivas[nombre] = propia;
            else if (ReglasCosmere.TryGetValue(nombre, out var cosmere)) efectivas[nombre] = cosmere;
        }
        foreach (var (nombre, rs) in propias)
            efectivas.TryAdd(nombre, rs);
        return efectivas;
    }

    // ── Habilidades personalizadas ───────────────────────────────────────────

    /// <summary>
    /// Grados del personaje en la habilidad personalizada (<c>HabilidadPersonalizada1..6</c>) cuyo nombre es
    /// <paramref name="habilidad"/>, sin distinguir mayúsculas ni tildes (el primer hueco que coincida), o <c>null</c> si no
    /// tiene ese hueco. Lo usan <see cref="GradosDe"/> y los derivados de cada mundo (Alomancia, Feruquimia).
    /// </summary>
    public static int? GradosHabilidadPersonalizada(CharacterEntity c, string? habilidad)
    {
        if (string.IsNullOrWhiteSpace(habilidad)) return null;
        var buscada = SinTildes(habilidad.Trim());
        (string Nombre, int Valor)[] huecos =
        [
            (c.HabilidadPersonalizada1, c.HabilidadPersonalizada1Valor),
            (c.HabilidadPersonalizada2, c.HabilidadPersonalizada2Valor),
            (c.HabilidadPersonalizada3, c.HabilidadPersonalizada3Valor),
            (c.HabilidadPersonalizada4, c.HabilidadPersonalizada4Valor),
            (c.HabilidadPersonalizada5, c.HabilidadPersonalizada5Valor),
            (c.HabilidadPersonalizada6, c.HabilidadPersonalizada6Valor),
        ];
        foreach (var (nombre, valor) in huecos)
        {
            if (!string.IsNullOrWhiteSpace(nombre) &&
                string.Equals(SinTildes(nombre.Trim()), buscada, StringComparison.OrdinalIgnoreCase))
                return valor;
        }
        return null;
    }

    private static string SinTildes(string texto)
    {
        var descompuesto = texto.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder(descompuesto.Length);
        foreach (var ch in descompuesto)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }
}
