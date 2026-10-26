using Messages.Characters.In;
using Messages.Database.Entities;
using Services.Characters;

namespace Services.Campaigns;

/// <summary>
/// Creación guiada de personajes (sección «Creación de personajes»): el director libera los pasos uno a uno (<c>Campaign.CreacionPaso</c>,
/// 0 = sin abrir) y los jugadores van completando su ficha. Cada mundo dice qué pasos tiene y en qué orden (<c>IWorldRules.PasosCreacion</c>,
/// como lista de ids de campo). Mientras <c>Character.CreacionCompletada</c> es falso y la campaña está en preparación, un jugador solo
/// responde los pasos liberados y solo cambia los ya respondidos que el director permite (<c>Campaign.CreacionCambiables</c>); el director
/// siempre puede. Al marcar «Personaje creado», la ficha vuelve a las reglas de siempre (libre hasta <see cref="CierreCampana"/>). Regla
/// del núcleo: un mundo sin pasos (Stormlight) no tiene creación guiada.
/// </summary>
public static class CreacionGuiada
{
    /// <summary>Ids de campo que admite la lista de pasos de un mundo. Un paso de respuesta del legado lleva su índice: <c>legadoRespuestas:0</c>.</summary>
    public const string Nombre = "name";
    public const string Legado = "legado";
    public const string Ascendencia = "ascendencia";
    public const string RespuestaLegado = "legadoRespuestas";

    /// <summary>Paso que guarda un dato propio de la creación (clave de <c>CreacionDatos</c>), no un campo de la ficha: <c>creacion:alomancia</c>.</summary>
    public const string Dato = "creacion";

    /// <summary>Prefijo del nombre con el que nace una ficha de la creación guiada («Personaje de &lt;jugador&gt;»): con él, el paso del nombre no está respondido.</summary>
    public const string NombreProvisional = "Personaje de ";

    public static string Respuesta(int indice) => $"{RespuestaLegado}:{indice}";

    public static string DatoDe(string clave) => $"{Dato}:{clave}";

    /// <summary>Si el paso del campo está liberado: su posición (desde 0) es menor que el número de pasos liberados.</summary>
    public static bool Liberado(IReadOnlyList<string> pasos, int pasoLiberado, string campo)
    {
        var i = IndiceDe(pasos, campo);
        return i >= 0 && i < pasoLiberado;
    }

    /// <summary>
    /// <c>PUT</c> de un jugador sobre un personaje en creación, con <see cref="ArgumentException"/> (400) que nombra el paso cuando no cabe:
    /// responder un paso (darle un valor) exige que esté liberado, y cambiar uno ya respondido exige además que el director lo permita
    /// (<paramref name="cambiables"/>). Vaciar un campo se admite siempre, esté o no liberado su paso: es lo que hace el jugador al cambiar
    /// de trasfondo (reinicia los pasos siguientes) y solo quita lo que él mismo escribió, aunque el director haya recogido el paso después,
    /// o el dato venga de una versión anterior de la lista de pasos. Un campo que no está en la lista de pasos no entra en la creación
    /// guiada y sigue las reglas de siempre. <c>Name</c> lo conserva el núcleo para los jugadores salvo en la creación guiada, así que aquí
    /// se comprueba como los demás (respondido cuando ya no es el provisional).
    /// </summary>
    public static void ValidarCambios(UpdateCharacterRequest r, CharacterEntity c, IReadOnlyList<string> pasos, int pasoLiberado,
        IReadOnlyList<string> cambiables)
    {
        for (var i = 0; i < pasos.Count; i++)
        {
            var campo = pasos[i];
            var (nuevo, actual) = campo switch
            {
                Nombre => (r.Name, c.Name),
                Legado => (r.Legado, c.Legado),
                Ascendencia => (r.Ascendencia, c.Ascendencia),
                _ when campo.StartsWith(RespuestaLegado + ":", StringComparison.Ordinal) =>
                    ValorRespuesta(r, c, int.Parse(campo[(RespuestaLegado.Length + 1)..])),
                _ when campo.StartsWith(Dato + ":", StringComparison.Ordinal) => ValorDato(r, c, campo[(Dato.Length + 1)..]),
                _ => (null, string.Empty),
            };
            if (!Responde(nuevo, actual)) continue;
            if (i >= pasoLiberado)
                throw new ArgumentException($"El director aún no ha liberado el paso {i + 1} de la creación ({campo}).");
            if (Respondido(campo, actual) && !cambiables.Contains(campo))
                throw new ArgumentException($"El director no permite cambiar el paso {i + 1} de la creación ({campo}) una vez respondido.");
        }
    }

    /// <summary>La petición da al campo un valor nuevo y no vacío (vaciarlo no es responder el paso).</summary>
    private static bool Responde(string? nuevo, string actual) => !string.IsNullOrWhiteSpace(nuevo) && nuevo != actual;

    /// <summary>El paso ya tiene respuesta: un valor no vacío, y en el nombre, uno que no sea el provisional.</summary>
    private static bool Respondido(string campo, string actual) =>
        campo == Nombre ? !EsNombreProvisional(actual) : !string.IsNullOrWhiteSpace(actual);

    public static bool EsNombreProvisional(string name) =>
        string.IsNullOrWhiteSpace(name) || name.Trim().StartsWith(NombreProvisional, StringComparison.Ordinal);

    private static int IndiceDe(IReadOnlyList<string> pasos, string campo)
    {
        for (var i = 0; i < pasos.Count; i++)
            if (pasos[i] == campo) return i;
        return -1;
    }

    /// <summary>Valor pedido (<c>null</c> = la petición no toca los datos) y guardado de un dato de la creación.</summary>
    private static (string? nuevo, string actual) ValorDato(UpdateCharacterRequest r, CharacterEntity c, string clave)
    {
        var guardados = CharacterJson.ParseCreacionDatos(c.CreacionDatos);
        var nuevo = r.CreacionDatos is null ? null : r.CreacionDatos.GetValueOrDefault(clave) ?? "";
        return (nuevo, guardados.GetValueOrDefault(clave) ?? "");
    }

    /// <summary>Valor pedido (<c>null</c> = la petición no toca las respuestas) y guardado de una respuesta del legado.</summary>
    private static (string? nuevo, string actual) ValorRespuesta(UpdateCharacterRequest r, CharacterEntity c, int indice)
    {
        var nueva = r.LegadoRespuestas is null ? null : indice < r.LegadoRespuestas.Count ? r.LegadoRespuestas[indice] : "";
        var actual = indice < c.LegadoRespuestas.Count ? c.LegadoRespuestas[indice] : "";
        return (nueva, actual);
    }
}
