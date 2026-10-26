using Messages.Characters.In;
using Messages.Database.Entities;
using Services.Characters;

namespace Services.Campaigns;

/// <summary>
/// Creación guiada de personajes (sección «Creación de personajes»): el director libera los pasos uno a uno (<c>Campaign.CreacionPaso</c>,
/// 0 = sin abrir) y los jugadores van completando su ficha. Cada mundo dice qué pasos tiene y en qué orden (<c>IWorldRules.PasosCreacion</c>,
/// como lista de ids de campo). Mientras <c>Character.CreacionCompletada</c> es falso y la campaña está en preparación, un jugador solo
/// cambia los campos de los pasos liberados; el director siempre puede. Al marcar «Personaje creado», la ficha vuelve a las reglas de
/// siempre (libre hasta <see cref="CierreCampana"/>). Regla del núcleo: un mundo sin pasos (Stormlight) no tiene creación guiada.
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

    public static string Respuesta(int indice) => $"{RespuestaLegado}:{indice}";

    public static string DatoDe(string clave) => $"{Dato}:{clave}";

    /// <summary>Si el paso del campo está liberado: su posición (desde 0) es menor que el número de pasos liberados.</summary>
    public static bool Liberado(IReadOnlyList<string> pasos, int pasoLiberado, string campo)
    {
        var i = IndiceDe(pasos, campo);
        return i >= 0 && i < pasoLiberado;
    }

    /// <summary>
    /// <c>PUT</c> de un jugador sobre un personaje en creación: cada campo que la petición cambia debe tener su paso liberado; si no,
    /// <see cref="ArgumentException"/> (400) que nombra el paso que falta. Un campo que no está en la lista de pasos no entra en la creación
    /// guiada y sigue las reglas de siempre. <c>Name</c> lo conserva el núcleo para los jugadores salvo en la creación guiada, así que aquí
    /// se comprueba como los demás.
    /// </summary>
    public static void ValidarCambios(UpdateCharacterRequest r, CharacterEntity c, IReadOnlyList<string> pasos, int pasoLiberado)
    {
        for (var i = Math.Max(pasoLiberado, 0); i < pasos.Count; i++)
        {
            var campo = pasos[i];
            var cambia = campo switch
            {
                Nombre => r.Name != c.Name,
                Legado => r.Legado is not null && r.Legado != c.Legado,
                Ascendencia => r.Ascendencia != c.Ascendencia,
                _ when campo.StartsWith(RespuestaLegado + ":", StringComparison.Ordinal) =>
                    CambiaRespuesta(r, c, int.Parse(campo[(RespuestaLegado.Length + 1)..])),
                _ when campo.StartsWith(Dato + ":", StringComparison.Ordinal) => CambiaDato(r, c, campo[(Dato.Length + 1)..]),
                _ => false,
            };
            if (cambia)
                throw new ArgumentException($"El director aún no ha liberado el paso {i + 1} de la creación ({campo}).");
        }
    }

    private static int IndiceDe(IReadOnlyList<string> pasos, string campo)
    {
        for (var i = 0; i < pasos.Count; i++)
            if (pasos[i] == campo) return i;
        return -1;
    }

    private static bool CambiaDato(UpdateCharacterRequest r, CharacterEntity c, string clave)
    {
        if (r.CreacionDatos is null) return false;
        var guardados = CharacterJson.ParseCreacionDatos(c.CreacionDatos);
        return (r.CreacionDatos.GetValueOrDefault(clave) ?? "") != (guardados.GetValueOrDefault(clave) ?? "");
    }

    private static bool CambiaRespuesta(UpdateCharacterRequest r, CharacterEntity c, int indice)
    {
        if (r.LegadoRespuestas is null) return false;
        var nueva = indice < r.LegadoRespuestas.Count ? r.LegadoRespuestas[indice] : "";
        var actual = indice < c.LegadoRespuestas.Count ? c.LegadoRespuestas[indice] : "";
        return nueva != actual;
    }
}
