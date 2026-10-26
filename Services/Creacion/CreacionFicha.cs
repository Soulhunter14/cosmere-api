using System.Reflection;
using System.Text.Json;
using Infrastructure.Data;
using Messages.Characters;
using Messages.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Services.Characters;
using Services.Worlds;

namespace Services.Creacion;

/// <summary>
/// Aplica a la ficha lo que la creación guiada dicta (<see cref="IWorldRules.FichaDeCreacion"/>): el flujo «Al elegir camino» de la ficha
/// (caminoMetalFlujo.ts del cliente) en la parte que necesita la creación, sobre la entidad, con la misma regla en el <c>PUT</c> de personajes
/// y en la sincronización al arrancar. Idempotente: aplicarlo dos veces deja la misma ficha. Como en la ficha, al deshacer se conservan las
/// metas ya creadas, los poderes de otro origen (clavos, lerasium, medallones) y el hueco de la habilidad investida si uno de ellos la sigue dando.
/// </summary>
public static class CreacionFicha
{
    /// <summary>Huecos de habilidad personalizada por código de atributo, los mismos que la ficha (ATRIBUTO_SLOTS de CharacterDetailPage): los cognitivos (INT, VOL) son el 2 y el 5.</summary>
    private static readonly IReadOnlyDictionary<string, int[]> HuecosPorAtributo = new Dictionary<string, int[]>
    {
        ["VEL"] = [1, 4], ["FUE"] = [1, 4], ["INT"] = [2, 5], ["VOL"] = [2, 5], ["DIS"] = [3, 6], ["PRE"] = [3, 6],
    };
    private static readonly int[] Huecos = [1, 2, 3, 4, 5, 6];
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Aplica <paramref name="plan"/> a <paramref name="c"/> y crea (o reutiliza) la meta que enlaza a los poderes nacientes. Devuelve si la
    /// ficha cambió. No guarda la ficha (lo hace el llamador); la meta nueva sí se guarda, para tener su id.
    /// </summary>
    public static async Task<bool> AplicarAsync(CosmereContext db, CharacterEntity c, FichaDeCreacion plan)
    {
        var antes = Huella(c);
        var poderesRaw = c.Poderes;
        var talentosRaw = c.Talentos;
        var poderes = CharacterJson.ParsePoderes(c.Poderes);
        var conservados = poderes.Where(p => p.Origen != "camino").ToList();
        var talentos = ParseTalentos(c.Talentos);
        talentos.RemoveAll(t => plan.TalentosDeCaminos.Contains(t));

        if (plan.CaminoMetal == "")
        {
            // Deshacer, como «Quitar camino» de la ficha
            if (!conservados.Any(p => p.Arte == plan.Habilidad.Arte && p.Origen is "clavo" or "lerasium"))
                foreach (var n in Huecos)
                    if (Leer(c, n).nombre == plan.Habilidad.Nombre) Escribir(c, n, "", 0, "");
            c.CaminoMetal = "";
            c.CaminoInicial = c.CaminoHeroico != "" ? "heroico" : "";
            c.Poderes = CharacterJson.SerializarPoderes(conservados);
        }
        else
        {
            // 1-2. Sin camino heroico, el de nacido del metal es el inicial sin pregunta; con él, lo decide el director (queda como esté)
            var inicial = c.CaminoHeroico == "" ? "metal" : c.CaminoInicial;
            c.CaminoMetal = plan.CaminoMetal;
            c.CaminoInicial = inicial;
            // Talento principal: concedido sin ocupar hueco (no se guarda) si el camino es el inicial; guardado, el primero, si no
            if (inicial != "metal" && plan.TalentoPrincipal is not null) talentos.Insert(0, plan.TalentoPrincipal);
            // 3. La habilidad investida en su hueco (el que ya la tiene o el primer hueco libre de su atributo), con el grado gratuito si procede.
            //    Sin hueco libre no se escribe: la ficha lo avisa al director (Q4); en una ficha de creación los huecos están vacíos
            var h = plan.Habilidad;
            var hueco = Huecos.FirstOrDefault(n => Leer(c, n).nombre == h.Nombre);
            if (hueco == 0) hueco = (HuecosPorAtributo.GetValueOrDefault(h.Codigo) ?? []).FirstOrDefault(n => Leer(c, n).nombre == "");
            if (hueco != 0) Escribir(c, hueco, h.Nombre, h.Grados + (h.GradoInicial && inicial == "metal" ? 1 : 0), h.Codigo);
            // 4. Los poderes del camino: nuevos y nacientes, o el mismo de antes con su estado de mesa y su meta; uno que el personaje ya
            //    tenga de otro origen (clavo, lerasium) sigue tal cual y no se duplica
            var nuevos = new List<PoderPersonaje>();
            foreach (var p in plan.Poderes)
            {
                var previo = poderes.FirstOrDefault(e => e.Arte == p.Arte && e.Metal == p.Metal);
                if (previo is null) nuevos.Add(p);
                else if (previo.Origen == "camino") nuevos.Add(previo);
            }
            // 5. La meta que entrena los poderes nacientes: una por personaje, reutilizada mientras siga activa
            var sinMeta = nuevos.Where(p => !p.Completo && p.MetaId is null).ToList();
            if (plan.Meta is not null && sinMeta.Count > 0)
            {
                var meta = await db.Metas.FirstOrDefaultAsync(m => m.CharacterId == c.Id && m.Titulo == plan.Meta.Titulo && m.Estado == "activa");
                if (meta is null)
                {
                    meta = new MetaEntity { CharacterId = c.Id, Titulo = plan.Meta.Titulo, Descripcion = plan.Meta.Descripcion };
                    db.Metas.Add(meta);
                    await db.SaveChangesAsync();
                }
                foreach (var p in sinMeta) p.MetaId = meta.Id;
            }
            c.Poderes = CharacterJson.SerializarPoderes(conservados.Concat(nuevos).ToList());
        }
        c.Talentos = JsonSerializer.Serialize(talentos, Json);

        if (Huella(c) == antes)
        {
            // Nada cambió de verdad: se dejan los textos tal cual estaban, para que EF no escriba
            c.Poderes = poderesRaw;
            c.Talentos = talentosRaw;
            return false;
        }
        return true;
    }

    /// <summary>Lo que la creación puede tocar, normalizado, para saber si la ficha cambió.</summary>
    private static string Huella(CharacterEntity c) => string.Join("|",
        c.CaminoMetal, c.CaminoInicial,
        CharacterJson.SerializarPoderes(CharacterJson.ParsePoderes(c.Poderes)),
        JsonSerializer.Serialize(ParseTalentos(c.Talentos), Json),
        string.Join(",", Huecos.Select(n => { var (nombre, valor, atributo) = Leer(c, n); return $"{nombre}:{valor}:{atributo}"; })));

    /// <summary>Lista de nombres de talento (JSON del cliente, <c>leerTalentos</c>); un texto vacío o ilegible vale como ninguno.</summary>
    private static List<string> ParseTalentos(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return [];
        try { return (JsonSerializer.Deserialize<List<string?>>(raw, Json) ?? []).OfType<string>().ToList(); }
        catch { return []; }
    }

    private static (string nombre, int valor, string atributo) Leer(CharacterEntity c, int n) => (
        ((string?)Prop($"HabilidadPersonalizada{n}").GetValue(c) ?? "").Trim(),
        (int)(Prop($"HabilidadPersonalizada{n}Valor").GetValue(c) ?? 0),
        (string?)Prop($"HabilidadPersonalizada{n}Atributo").GetValue(c) ?? "");

    private static void Escribir(CharacterEntity c, int n, string nombre, int valor, string atributo)
    {
        Prop($"HabilidadPersonalizada{n}").SetValue(c, nombre);
        Prop($"HabilidadPersonalizada{n}Valor").SetValue(c, valor);
        Prop($"HabilidadPersonalizada{n}Atributo").SetValue(c, atributo);
    }

    private static PropertyInfo Prop(string nombre) =>
        typeof(CharacterEntity).GetProperty(nombre) ?? throw new InvalidOperationException($"CharacterEntity.{nombre} no existe.");
}
