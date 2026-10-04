using System.Text.Json;
using Messages.Characters;

namespace Services.Characters;

/// <summary>
/// Lectura y escritura de las columnas JSON del personaje (<c>Poderes</c>, <c>Recursos</c>), compartida por
/// <see cref="CharacterService"/> y las reglas de mundo (mismo ensamblado; <c>MetaService</c> no la usa: delega en los hooks
/// del mundo). La lectura es tolerante, con el mismo patrón que <c>CharacterService.ParseTalentos</c>: <c>null</c>, <c>""</c>
/// o JSON inválido devuelven una colección vacía.
/// </summary>
internal static class CharacterJson
{
    // camelCase y lectura sin distinguir mayúsculas: lo mismo que ASP.NET Core aplica por defecto en los controladores.
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static List<PoderPersonaje> ParsePoderes(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return [];
        try { return (JsonSerializer.Deserialize<List<PoderPersonaje?>>(raw, Json) ?? []).OfType<PoderPersonaje>().ToList(); }
        catch { return []; }
    }

    public static Dictionary<string, decimal> ParseRecursos(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return new();
        try { return JsonSerializer.Deserialize<Dictionary<string, decimal>>(raw, Json) ?? new(); }
        catch { return new(); }
    }

    public static string SerializarPoderes(List<PoderPersonaje> poderes) => JsonSerializer.Serialize(poderes, Json);

    public static string SerializarRecursos(Dictionary<string, decimal> recursos) => JsonSerializer.Serialize(recursos, Json);

    /// <summary>
    /// Lista de poderes que resulta de un <c>PUT</c> (§5.1). <paramref name="entrantes"/> <c>null</c> conserva la guardada.
    /// Si no, manda la lista del cuerpo, pero de cada poder solo se escriben <c>Arte</c>, <c>Metal</c>, <c>Origen</c> y
    /// <c>MetaId</c>: un poder que ya existía por <c>(Arte, Metal)</c> conserva <c>Completo</c>, <c>Cargas</c>,
    /// <c>AjusteCargasMax</c>, <c>Viales</c> y <c>Desprovisto</c> (estado de mesa: solo <c>PATCH …/recursos</c> y la conclusión
    /// de su meta lo cambian), y uno nuevo nace con los valores iniciales. Después normaliza (§5.3, P6): un poder que no viene
    /// del camino es completo y sin meta (L.290 / PDF 296; L.295 / PDF 301; medallón [inferido]) y la alomancia de atium es
    /// siempre completa (L.177 / PDF 183). Los duplicados <c>(Arte, Metal)</c> no se eliminan: los rechaza la validación del
    /// mundo. Una entrada <c>null</c> en el cuerpo da <see cref="ArgumentException"/> (400).
    /// </summary>
    public static List<PoderPersonaje> FusionarPoderes(List<PoderPersonaje> existentes, List<PoderPersonaje>? entrantes)
    {
        if (entrantes is null) return existentes;

        var fusion = new List<PoderPersonaje>(entrantes.Count);
        foreach (var p in entrantes)
        {
            if (p is null) throw new ArgumentException("Invalid Poderes: null entry.");

            var guardado = existentes.FirstOrDefault(e => e.Arte == p.Arte && e.Metal == p.Metal);
            var poder = new PoderPersonaje
            {
                Arte = p.Arte,
                Metal = p.Metal,
                Origen = p.Origen,
                MetaId = p.MetaId,
                // El Completo del cuerpo se ignora: se conserva el guardado o nace naciente.
                Completo = guardado?.Completo ?? false,
                // La DJ elige las cargas iniciales del medallón, casi siempre 8 (L.293 / PDF 299).
                Cargas = guardado?.Cargas ?? (p.Origen == "medallon" ? 8 : 0),
                AjusteCargasMax = guardado?.AjusteCargasMax ?? 0,
                Viales = guardado?.Viales ?? 0,
                Desprovisto = guardado?.Desprovisto ?? false,
            };

            if (poder.Origen != "camino")
            {
                poder.Completo = true;
                poder.MetaId = null;
            }
            if (poder.Arte == "alomancia" && poder.Metal == "atium")
                poder.Completo = true;

            fusion.Add(poder);
        }
        return fusion;
    }
}
