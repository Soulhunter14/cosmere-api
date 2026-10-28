using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Messages.Creacion;
using Messages.Database.Entities;
using Services.Characters;

namespace Services.Creacion;

/// <summary>
/// Escribe en la ficha lo que da una decisión de la sesión (<see cref="EfectosFicha"/>) y lo deshace cuando la decisión cambia o se
/// quita. Atributos sin pasar de 3 y grados sin pasar de 2 por habilidad (los topes de la creación del Manual), pericias y objetos sin
/// repetir; lo que ya supera el tope no baja, y lo que no encuentra sitio se devuelve para anotarlo a mano. Por cada fila queda lo pedido y
/// lo hecho (<see cref="RegistroAplicado"/>, JSON en <c>CharacterRespuestaEntity.Aplicado</c>), con las claves resueltas a columnas de la
/// ficha, para revertir exactamente lo que se sumó. Las metas las crea y borra el servicio (necesitan la base de datos).
/// </summary>
public static class AplicadorFicha
{
    public const int TopeAtributo = 3;
    public const int TopeHabilidad = 2;
    public const string ClaveCaminoRecomendado = "caminoRecomendado";
    public const string ClaveMetalRecomendado = "metalRecomendado";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly string[] Atributos = ["Fuerza", "Velocidad", "Intelecto", "Voluntad", "Discernimiento", "Presencia"];
    private static readonly string[] Habilidades =
    [
        "Agilidad", "ArmasLigeras", "ArmasPesadas", "Atletismo", "Hurto", "Sigilo", "Deduccion", "Disciplina", "Intimidacion", "Manufactura",
        "Medicina", "Conocimiento", "Engano", "Liderazgo", "Percepcion", "Perspicacia", "Persuasion", "Supervivencia",
    ];

    /// <summary>Lo pedido por una decisión y lo que de verdad se escribió (claves = columnas de la ficha; metas = títulos creados).</summary>
    public record RegistroAplicado(EfectosFicha Pedido, EfectosFicha Hecho);

    /// <summary>Aplica lo pedido (salvo las metas) y devuelve lo hecho, para revertirlo igual más adelante.</summary>
    public static EfectosFicha AplicarFila(CharacterEntity c, EfectosFicha pedido, List<string> lineas, List<string> ignorado)
    {
        var hecho = new EfectosFicha();
        foreach (var (clave, suma) in pedido.Atributos)
        {
            if (suma <= 0) continue;
            var prop = Propiedad(Atributos, clave);
            if (prop is null) { ignorado.Add($"Atributo desconocido: {clave} +{suma}"); continue; }
            var d = Sumar(c, prop, suma, TopeAtributo, lineas, ignorado, prop.Name);
            if (d > 0) hecho.Atributos[prop.Name] = d;
        }
        foreach (var (clave, suma) in pedido.Habilidades)
        {
            if (suma <= 0) continue;
            var prop = Propiedad(Habilidades, clave) ?? PropiedadPersonalizada(c, clave);
            if (prop is null) { ignorado.Add($"Habilidad sin sitio en la ficha: {clave} +{suma}"); continue; }
            var d = Sumar(c, prop, suma, TopeHabilidad, lineas, ignorado, clave);
            if (d > 0) hecho.Habilidades[prop.Name] = d;
        }
        foreach (var p in pedido.Pericias.Select(x => x.Trim()).Where(x => x != ""))
        {
            if (c.Pericias.Any(x => Igual(x, p))) continue;
            c.Pericias = [.. c.Pericias, p];
            hecho.Pericias.Add(p);
            lineas.Add($"Pericia {p}");
        }
        foreach (var o in pedido.Objetos.Select(x => x.Trim()).Where(x => x != ""))
        {
            if (c.Equipment.Any(x => Igual(x, o))) continue;
            c.Equipment = [.. c.Equipment, o];
            hecho.Objetos.Add(o);
            lineas.Add($"Objeto {o}");
        }
        return hecho;
    }

    /// <summary>Deshace lo hecho por una decisión que cambia o se quita (salvo las metas): resta sin bajar de 0 y quita lo que puso.</summary>
    public static void RevertirFila(CharacterEntity c, EfectosFicha hecho, List<string> lineas)
    {
        foreach (var (nombre, d) in hecho.Atributos.Concat(hecho.Habilidades))
        {
            var prop = typeof(CharacterEntity).GetProperty(nombre);
            if (prop is null || prop.PropertyType != typeof(int) || d <= 0) continue;
            var actual = (int)(prop.GetValue(c) ?? 0);
            var nuevo = Math.Max(0, actual - d);
            if (nuevo == actual) continue;
            prop.SetValue(c, nuevo);
            lineas.Add($"{nombre} {actual} → {nuevo}");
        }
        foreach (var p in hecho.Pericias)
        {
            if (!c.Pericias.Any(x => Igual(x, p))) continue;
            c.Pericias = c.Pericias.Where(x => !Igual(x, p)).ToList();
            lineas.Add($"Pericia {p} quitada");
        }
        foreach (var o in hecho.Objetos)
        {
            if (!c.Equipment.Any(x => Igual(x, o))) continue;
            c.Equipment = c.Equipment.Where(x => !Igual(x, o)).ToList();
            lineas.Add($"Objeto {o} quitado");
        }
    }

    /// <summary>Guarda el camino y el metal recomendados (los más marcados) en los datos de la creación; vacíos = sin cambio.</summary>
    public static void Recomendar(CharacterEntity c, string camino, string metal, List<string> lineas)
    {
        camino = camino.Trim();
        metal = metal.Trim();
        if (camino == "" && metal == "") return;
        var datos = CharacterJson.ParseCreacionDatos(c.CreacionDatos);
        if (camino != "" && datos.GetValueOrDefault(ClaveCaminoRecomendado) != camino) { datos[ClaveCaminoRecomendado] = camino; lineas.Add($"Camino recomendado: {camino}"); }
        if (metal != "" && datos.GetValueOrDefault(ClaveMetalRecomendado) != metal) { datos[ClaveMetalRecomendado] = metal; lineas.Add($"Metal recomendado: {metal}"); }
        c.CreacionDatos = CharacterJson.SerializarCreacionDatos(datos);
    }

    // ── El registro por fila ─────────────────────────────────────────────────

    public static RegistroAplicado? LeerAplicado(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || raw.Trim() == "{}") return null;
        try
        {
            var r = JsonSerializer.Deserialize<RegistroAplicado>(raw, Json);
            return r is null ? null : new RegistroAplicado(r.Pedido ?? new EfectosFicha(), r.Hecho ?? new EfectosFicha());
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static string EscribirAplicado(RegistroAplicado r) => JsonSerializer.Serialize(r, Json);

    /// <summary>Dos peticiones piden lo mismo (sin importar el orden ni mayúsculas, acentos o espacios).</summary>
    public static bool MismoPedido(EfectosFicha a, EfectosFicha b) => Canonico(a) == Canonico(b);

    private static string Canonico(EfectosFicha e)
    {
        var sb = new StringBuilder();
        foreach (var (k, v) in e.Atributos.OrderBy(x => Plegar(x.Key))) sb.Append("a:").Append(Plegar(k)).Append('=').Append(v).Append(';');
        foreach (var (k, v) in e.Habilidades.OrderBy(x => Plegar(x.Key))) sb.Append("h:").Append(Plegar(k)).Append('=').Append(v).Append(';');
        foreach (var x in e.Pericias.Select(Plegar).OrderBy(x => x)) sb.Append("p:").Append(x).Append(';');
        foreach (var x in e.Metas.Select(Plegar).OrderBy(x => x)) sb.Append("m:").Append(x).Append(';');
        foreach (var x in e.Objetos.Select(Plegar).OrderBy(x => x)) sb.Append("o:").Append(x).Append(';');
        return sb.ToString();
    }

    // ── Ayudantes ───────────────────────────────────────────────────────────

    /// <summary>Suma con tope y devuelve lo que de verdad subió (0 si ya estaba en el tope o por encima).</summary>
    private static int Sumar(CharacterEntity c, PropertyInfo prop, int suma, int tope, List<string> lineas, List<string> ignorado, string etiqueta)
    {
        var actual = (int)(prop.GetValue(c) ?? 0);
        var nuevo = actual >= tope ? actual : Math.Min(tope, actual + suma);
        if (nuevo == actual)
        {
            ignorado.Add($"{etiqueta} +{suma}: ya está en {actual} (tope {tope})");
            return 0;
        }
        prop.SetValue(c, nuevo);
        lineas.Add($"{etiqueta} {actual} → {nuevo}" + (nuevo < actual + suma ? $" (tope {tope})" : ""));
        return nuevo - actual;
    }

    private static PropertyInfo? Propiedad(string[] nombres, string clave)
    {
        var n = nombres.FirstOrDefault(x => Igual(x, clave));
        return n is null ? null : typeof(CharacterEntity).GetProperty(n);
    }

    /// <summary>Una habilidad personalizada de la ficha (Alomancia, una profesión…) por su nombre: su valor es «HabilidadPersonalizada{i}Valor».</summary>
    private static PropertyInfo? PropiedadPersonalizada(CharacterEntity c, string clave)
    {
        for (var i = 1; i <= 6; i++)
        {
            var nombre = typeof(CharacterEntity).GetProperty($"HabilidadPersonalizada{i}")?.GetValue(c) as string;
            if (!string.IsNullOrWhiteSpace(nombre) && Igual(nombre, clave))
                return typeof(CharacterEntity).GetProperty($"HabilidadPersonalizada{i}Valor");
        }
        return null;
    }

    /// <summary>Compara sin mayúsculas, acentos ni espacios («Engaño» = «engano», «armasLigeras» = «ArmasLigeras»).</summary>
    public static bool Igual(string a, string b) => Plegar(a) == Plegar(b);

    private static string Plegar(string s)
    {
        var sb = new StringBuilder();
        foreach (var ch in s.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark || char.IsWhiteSpace(ch)) continue;
            sb.Append(char.ToLowerInvariant(ch));
        }
        return sb.ToString();
    }
}
