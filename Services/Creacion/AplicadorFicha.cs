using System.Globalization;
using System.Reflection;
using System.Text;
using Messages.Creacion.In;
using Messages.Database.Entities;
using Services.Characters;

namespace Services.Creacion;

/// <summary>
/// Escribe en la ficha lo que dan las decisiones de la sesión (<see cref="AplicarRespuestasRequest"/>): atributos sin pasar de 3 y grados
/// sin pasar de 2 por habilidad (los topes de la creación del Manual), pericias y objetos sin repetir, y la recomendación de camino y
/// metal en los datos de la creación. Lo que ya supera el tope no baja; lo que no encuentra sitio se devuelve para anotarlo a mano.
/// Las metas las crea el servicio (necesitan la base de datos).
/// </summary>
public static class AplicadorFicha
{
    public const int TopeAtributo = 3;
    public const int TopeHabilidad = 2;
    public const string ClaveCaminoRecomendado = "caminoRecomendado";
    public const string ClaveMetalRecomendado = "metalRecomendado";

    private static readonly string[] Atributos = ["Fuerza", "Velocidad", "Intelecto", "Voluntad", "Discernimiento", "Presencia"];
    private static readonly string[] Habilidades =
    [
        "Agilidad", "ArmasLigeras", "ArmasPesadas", "Atletismo", "Hurto", "Sigilo", "Deduccion", "Disciplina", "Intimidacion", "Manufactura",
        "Medicina", "Conocimiento", "Engano", "Liderazgo", "Percepcion", "Perspicacia", "Persuasion", "Supervivencia",
    ];

    public static void Aplicar(CharacterEntity c, AplicarRespuestasRequest r, List<string> aplicado, List<string> ignorado)
    {
        foreach (var (clave, suma) in r.Atributos)
        {
            if (suma <= 0) continue;
            var prop = Propiedad(Atributos, clave);
            if (prop is null) { ignorado.Add($"Atributo desconocido: {clave} +{suma}"); continue; }
            Sumar(c, prop, suma, TopeAtributo, aplicado, ignorado, prop.Name);
        }
        foreach (var (clave, suma) in r.Habilidades)
        {
            if (suma <= 0) continue;
            var prop = Propiedad(Habilidades, clave) ?? PropiedadPersonalizada(c, clave);
            if (prop is null) { ignorado.Add($"Habilidad sin sitio en la ficha: {clave} +{suma}"); continue; }
            Sumar(c, prop, suma, TopeHabilidad, aplicado, ignorado, clave);
        }
        foreach (var p in r.Pericias.Select(x => x.Trim()).Where(x => x != ""))
        {
            if (c.Pericias.Any(x => Igual(x, p))) continue;
            c.Pericias = [.. c.Pericias, p];
            aplicado.Add($"Pericia {p}");
        }
        foreach (var o in r.Objetos.Select(x => x.Trim()).Where(x => x != ""))
        {
            if (c.Equipment.Any(x => Igual(x, o))) continue;
            c.Equipment = [.. c.Equipment, o];
            aplicado.Add($"Objeto {o}");
        }
        var camino = r.CaminoRecomendado.Trim();
        var metal = r.MetalRecomendado.Trim();
        if (camino != "" || metal != "")
        {
            var datos = CharacterJson.ParseCreacionDatos(c.CreacionDatos);
            if (camino != "") { datos[ClaveCaminoRecomendado] = camino; aplicado.Add($"Camino recomendado: {camino}"); }
            if (metal != "") { datos[ClaveMetalRecomendado] = metal; aplicado.Add($"Metal recomendado: {metal}"); }
            c.CreacionDatos = CharacterJson.SerializarCreacionDatos(datos);
        }
    }

    private static void Sumar(CharacterEntity c, PropertyInfo prop, int suma, int tope, List<string> aplicado, List<string> ignorado, string etiqueta)
    {
        var actual = (int)(prop.GetValue(c) ?? 0);
        var nuevo = actual >= tope ? actual : Math.Min(tope, actual + suma);
        if (nuevo == actual)
        {
            ignorado.Add($"{etiqueta} +{suma}: ya está en {actual} (tope {tope})");
            return;
        }
        prop.SetValue(c, nuevo);
        aplicado.Add($"{etiqueta} {actual} → {nuevo}" + (nuevo < actual + suma ? $" (tope {tope})" : ""));
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
