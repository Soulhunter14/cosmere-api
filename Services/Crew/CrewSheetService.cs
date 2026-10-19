using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Infrastructure.Data;
using Messages.Crew.In;
using Messages.Crew.Out;
using Messages.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Services.Characters;
using Services.Worlds;

namespace Services.Crew;

public class CrewSheetService(CosmereContext db, IWorldRulesProvider reglas) : ICrewSheetService
{
    /// <summary>Largest accepted sheet, in characters of its JSON text.</summary>
    public const int MaxStateLength = 1_000_000;
    /// <summary>Largest accepted GM part, in characters of its JSON text.</summary>
    public const int MaxGmStateLength = 200_000;
    /// <summary>Movements kept in the sheet's ledger (the web keeps the same number: MAX_REGISTRO in src/pages/banda/estado.ts).</summary>
    private const int MaxRegistro = 400;
    /// <summary>Largest accepted lot of the store, in characters of its JSON text.</summary>
    private const int MaxLoteLength = 20_000;

    public async Task<CrewSheetResponse> GetAsync(long campaignId, long userId)
    {
        var role = await RoleAsync(campaignId, userId);
        var row = await db.CrewSheets.AsNoTracking().FirstOrDefaultAsync(c => c.CampaignId == campaignId);

        if (role == "gm")
            return row is null ? EmptyResponse() : MapToResponse(row, forGm: true);

        // A player reads the sheet only when the GM has shared it with them, and never the GM part
        EnsureShared(row, userId);
        return MapToResponse(row!, forGm: false);
    }

    public async Task<CrewSheetSaveResult> SaveAsync(long campaignId, SaveCrewSheetRequest request, long userId)
    {
        var role = await RoleAsync(campaignId, userId);
        var esDirector = role == "gm";

        var json = ValidJson(request.State, MaxStateLength, "State");
        // Only the GM writes the private part; a player's request never touches it, whatever it carries
        var gmJson = esDirector && request.GmState is { ValueKind: not (JsonValueKind.Null or JsonValueKind.Undefined) } gm
            ? ValidJson(gm, MaxGmStateLength, "GmState")
            : null;

        var row = await db.CrewSheets.FirstOrDefaultAsync(c => c.CampaignId == campaignId);
        if (!esDirector)
        {
            // A player acts on a sheet that exists and is shared with them (creating it is the GM's)
            EnsureShared(row, userId);
            if (row!.Version == 0) throw new UnauthorizedAccessException("The crew sheet has not been created yet.");
        }

        if (row is null)
        {
            // First save: the client must have read the empty sheet, which is version 0.
            if (request.Version != 0)
                return Conflict(stored: null, esDirector);

            row = new CrewSheetEntity
            {
                CampaignId = campaignId, State = json, GmState = gmJson ?? "{}", Version = 1, UpdatedAt = DateTime.UtcNow,
            };
            db.CrewSheets.Add(row);
        }
        else
        {
            if (row.Version != request.Version)
                return Conflict(row, esDirector);

            row.State = json;
            if (gmJson is not null)
                row.GmState = gmJson;
            row.UpdatedAt = DateTime.UtcNow;
            // Version is a concurrency token: EF adds `WHERE "Version" = <the version read>` to the UPDATE.
            row.Version++;
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException e) when (IsLostRace(e))
        {
            // Lost a race, and nothing of ours was written: another save took the version first, or two first writes collided on
            // the primary key. Answer with what is stored now, as for any other conflict.
            db.ChangeTracker.Clear();
            var stored = await db.CrewSheets.AsNoTracking().FirstOrDefaultAsync(c => c.CampaignId == campaignId);
            return Conflict(stored, esDirector);
        }

        return new CrewSheetSaveResult(true, MapToResponse(row, esDirector));
    }

    public async Task<CrewSheetResponse> ShareAsync(long campaignId, ShareCrewSheetRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        var ids = request.UserIds.Distinct().ToList();
        var players = await db.CampaignMembers
            .Where(m => m.CampaignId == campaignId && m.Role == "player" && ids.Contains(m.UserId))
            .Select(m => m.UserId)
            .ToListAsync();
        if (players.Count != ids.Count)
            throw new ArgumentException("The crew sheet can only be shared with players of the campaign.");
        players.Sort();

        // The row may be written by a save of the sheet at the same time (the version token makes this update fail then):
        // read it again and apply the list on top, a couple of times at most.
        for (var attempt = 1; ; attempt++)
        {
            var row = await db.CrewSheets.FirstOrDefaultAsync(c => c.CampaignId == campaignId);
            if (row is null)
            {
                // Shared before the first save: the row starts with the empty sheet at version 0, so that first save still sends 0
                row = new CrewSheetEntity { CampaignId = campaignId, SharedWith = players, Version = 0, UpdatedAt = DateTime.UtcNow };
                db.CrewSheets.Add(row);
            }
            else
            {
                row.SharedWith = players;
            }

            try
            {
                await db.SaveChangesAsync();
                return MapToResponse(row, forGm: true);
            }
            catch (DbUpdateException e) when (attempt < 3 && IsLostRace(e))
            {
                db.ChangeTracker.Clear();
            }
        }
    }

    public async Task<CrewSheetResponse> AportarAsync(long campaignId, AportarCrewRequest request, long userId)
    {
        var esDirector = await RoleAsync(campaignId, userId) == "gm";

        var cantidad = request.Cantidad;
        if (cantidad <= 0 || decimal.Round(cantidad, 2) != cantidad)
            throw new ArgumentException("The amount must be more than 0, with at most 2 decimals.");
        var motivo = TextoRegistro(request.Motivo, "Aporte a la tesorería");

        var mundo = reglas.Get(await db.Campaigns.AsNoTracking().Where(c => c.Id == campaignId).Select(c => c.World).FirstOrDefaultAsync());
        var claveDinero = mundo.ClaveDinero ?? throw new ArgumentException("The money of this world cannot go to a crew treasury.");
        var autor = await AutorAsync(userId);

        // The money leaves the purse and enters the treasury together, or not at all
        await using var tx = await db.Database.BeginTransactionAsync();
        var character = await PersonajeBloqueadoAsync(campaignId, request.CharacterId, esDirector, userId, "You can only give money from your own character.");
        var row = await HojaBloqueadaAsync(campaignId, esDirector, userId);

        var recursos = CharacterJson.ParseRecursos(character.Recursos);
        var bolsa = recursos.GetValueOrDefault(claveDinero);
        if (bolsa < cantidad)
            throw new ArgumentException("The character does not have that much money.");
        recursos[claveDinero] = decimal.Round(bolsa - cantidad, 2);
        character.Recursos = CharacterJson.SerializarRecursos(recursos);
        character.UpdatedAt = DateTime.UtcNow;

        // The server only touches two fields of the sheet here: the treasury and the ledger (the same shape the web writes)
        var hoja = Hoja(row);
        hoja["tesoreria"] = decimal.Round((Numero(hoja["tesoreria"]) ?? 0m) + cantidad, 2);
        Anotar(hoja, autor, "tesoreria", Actor(character), motivo, cantidad);
        return await GuardarAsync(row, hoja, tx, esDirector);
    }

    public async Task<CrewSheetResponse> EntrarAlmacenAsync(long campaignId, EntrarAlmacenRequest request, long userId)
    {
        var esDirector = await RoleAsync(campaignId, userId) == "gm";

        var lista = ListaValida(request.Lista);
        var nombre = (request.Nombre ?? "").Trim();
        if (nombre.Length is 0 or > 120)
            throw new ArgumentException("The item's name is not valid.");
        if (request.Cantidad is < 1 or > 999)
            throw new ArgumentException("The quantity goes from 1 to 999.");
        var lote = JsonNode.Parse(ValidJson(request.Lote, MaxLoteLength, "Lote"))!.AsObject();
        var motivo = TextoRegistro(request.Motivo, $"Entra en el almacén: {nombre}");
        var motivoVenta = TextoRegistro(request.MotivoVenta, "");
        var autor = await AutorAsync(userId);

        // The units leave the inventory and enter the store together, or not at all
        await using var tx = await db.Database.BeginTransactionAsync();
        var character = await PersonajeBloqueadoAsync(campaignId, request.CharacterId, esDirector, userId, "You can only store goods from your own character.");
        var row = await HojaBloqueadaAsync(campaignId, esDirector, userId);

        // One inventory entry per unit; the worn armor comes off when none of it is left
        var inventario = new List<string>(ListaDe(character, lista));
        if (inventario.Count(n => n == nombre) < request.Cantidad)
            throw new ArgumentException("The character does not carry that many.");
        for (var i = 0; i < request.Cantidad; i++)
            inventario.RemoveAt(inventario.LastIndexOf(nombre));
        AsignarLista(character, lista, inventario);
        if (lista == "armor" && character.EquippedArmor == nombre && !inventario.Contains(nombre))
        {
            character.EquippedArmor = "";
            character.Desvio = 0;
        }
        character.UpdatedAt = DateTime.UtcNow;

        // The lot as the web built it, with what the server knows fixed: who, when, what, how many, from which list; on sale only
        // with a value and a price
        var hoja = Hoja(row);
        var lotes = Almacen(hoja);
        var propio = Cadena(lote["id"]);
        var id = propio is { Length: > 0 and <= 40 } && !lotes.Any(m => Cadena(m["id"]) == propio) ? propio : NuevoId();
        var aLaVenta = Cadena(lote["situacion"]) == "en-venta" && Numero(lote["precio"]) > 0 && Numero(lote["valor"]) > 0;
        lote["id"] = id;
        lote["nombre"] = nombre;
        lote["cantidad"] = request.Cantidad;
        lote["lista"] = lista;
        lote["situacion"] = aLaVenta ? "en-venta" : "guardada";
        if (!aLaVenta) lote["precio"] = null;
        lote["semanasALaVenta"] = 0;
        lote["traidaPor"] = Actor(character);
        lote["entradaEn"] = DateTime.UtcNow.ToString("O");
        lote["puestaPor"] = aLaVenta ? Actor(character) : null;
        lote["cerradaEn"] = null;
        lote["importeVenta"] = null;

        // The same goods join the open lot that already holds them, whoever brought each (the ledger says it); the web joins them
        // with the same rule, so the lot on screen while this request travels is the one it gets back
        JuntarEnAlmacen(lotes, lote);
        hoja["almacen"] = new JsonArray(lotes.Cast<JsonNode?>().ToArray());

        Anotar(hoja, autor, "almacen", Actor(character), motivo);
        if (aLaVenta && motivoVenta.Length > 0)
            Anotar(hoja, autor, "almacen", Actor(character), motivoVenta);
        return await GuardarAsync(row, hoja, tx, esDirector);
    }

    public async Task<CrewSheetResponse> SacarAlmacenAsync(long campaignId, string loteId, SacarAlmacenRequest request, long userId)
    {
        var esDirector = await RoleAsync(campaignId, userId) == "gm";
        var motivo = TextoRegistro(request.Motivo, "Sale del almacén");
        var autor = await AutorAsync(userId);

        // The lot closes and its units enter the inventory together, or not at all
        await using var tx = await db.Database.BeginTransactionAsync();
        var character = await PersonajeBloqueadoAsync(campaignId, request.CharacterId, esDirector, userId, "You can only take goods for your own character.");
        var row = await HojaBloqueadaAsync(campaignId, esDirector, userId);

        // The lots as the web shows them (the same goods saved apart, joined): the lot taken out carries all their units
        var hoja = Hoja(row);
        var lotes = Almacen(hoja);
        hoja["almacen"] = new JsonArray(lotes.Cast<JsonNode?>().ToArray());
        var lote = lotes.FirstOrDefault(m => Cadena(m["id"]) == loteId) ?? throw new KeyNotFoundException("Lot not found.");
        if (Cadena(lote["situacion"]) is not ("guardada" or "en-venta"))
            throw new ArgumentException("That lot is no longer in the store.");
        var nombre = (Cadena(lote["nombre"]) ?? "").Trim();
        if (nombre.Length == 0)
            throw new ArgumentException("The lot has no name.");
        var cantidad = (int)Math.Clamp(Numero(lote["cantidad"]) ?? 1m, 1m, 999m);

        // Its units go to the list they belong to: the one they came from, or the one of their price-table item, or the gear
        var lista = ListaDelLote(lote);
        var inventario = new List<string>(ListaDe(character, lista));
        inventario.AddRange(Enumerable.Repeat(nombre, cantidad));
        AsignarLista(character, lista, inventario);
        character.UpdatedAt = DateTime.UtcNow;

        lote["situacion"] = "retirada";
        lote["precio"] = null;
        lote["cerradaEn"] = DateTime.UtcNow.ToString("O");
        Anotar(hoja, autor, "almacen", Actor(character), motivo);
        return await GuardarAsync(row, hoja, tx, esDirector);
    }

    // ─── Helpers of the operations the server makes on the sheet together with a character ───

    private static readonly string[] Listas = ["weapons", "armor", "equipment"];

    private static string ListaValida(string? lista) =>
        lista is not null && Listas.Contains(lista) ? lista : throw new ArgumentException("The inventory list must be weapons, armor or equipment.");

    private static List<string> ListaDe(CharacterEntity c, string lista) => lista switch
    {
        "weapons" => c.Weapons,
        "armor" => c.Armor,
        _ => c.Equipment,
    };

    /// <summary>Sets a new list, so that EF sees the change of the array column</summary>
    private static void AsignarLista(CharacterEntity c, string lista, List<string> valor)
    {
        switch (lista)
        {
            case "weapons": c.Weapons = valor; break;
            case "armor": c.Armor = valor; break;
            default: c.Equipment = valor; break;
        }
    }

    /// <summary>The inventory list a lot goes back to: the one it came from, or its price-table item's, or the gear</summary>
    private static string ListaDelLote(JsonObject lote)
    {
        if (Cadena(lote["lista"]) is { } lista && Listas.Contains(lista)) return lista;
        return Cadena((lote["deCatalogo"] as JsonObject)?["tipo"]) switch
        {
            "arma" => "weapons",
            "armadura" => "armor",
            _ => "equipment",
        };
    }

    /// <summary>
    /// The character, locked until the commit (always before the crew's sheet). Same filters and same 404/403 as the character's own
    /// table actions (PATCH …/recursos): a PC of the campaign; a player acts only for their own
    /// </summary>
    private async Task<CharacterEntity> PersonajeBloqueadoAsync(long campaignId, long characterId, bool esDirector, long userId, string soloSuyo)
    {
        var character = await db.Characters
            .FromSqlInterpolated($"SELECT * FROM \"Characters\" WHERE \"Id\" = {characterId} AND \"CampaignId\" = {campaignId} AND NOT \"IsNpc\" FOR UPDATE")
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Character not found.");
        if (!esDirector && character.OwnerId != userId)
            throw new UnauthorizedAccessException(soloSuyo);
        // A dead character's sheet is read-only: no money or goods move to or from it (409)
        MuertePersonaje.EnsureVivo(character);
        return character;
    }

    /// <summary>The crew's sheet, locked until the commit: it must exist and, for a player, be shared with them</summary>
    private async Task<CrewSheetEntity> HojaBloqueadaAsync(long campaignId, bool esDirector, long userId)
    {
        var row = await db.CrewSheets
            .FromSqlInterpolated($"SELECT * FROM \"CrewSheets\" WHERE \"CampaignId\" = {campaignId} FOR UPDATE")
            .FirstOrDefaultAsync();
        if (!esDirector) EnsureShared(row, userId);
        if (row is null || row.Version == 0)
            throw new ArgumentException("The crew sheet has not been created yet.");
        return row;
    }

    /// <summary>Stores the sheet the server edited as a new version, commits the transaction and answers with it</summary>
    private async Task<CrewSheetResponse> GuardarAsync(CrewSheetEntity row, JsonObject hoja, IDbContextTransaction tx, bool esDirector)
    {
        var json = hoja.ToJsonString();
        if (json.Length > MaxStateLength)
            throw new ArgumentException("State is too large.");
        row.State = json;
        row.UpdatedAt = DateTime.UtcNow;
        row.Version++;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return MapToResponse(row, esDirector);
    }

    /// <summary>A movement at the top of the ledger, with the shape the web writes (src/pages/banda/estado.ts, Movimiento)</summary>
    private static void Anotar(JsonObject hoja, string autor, string tipo, JsonObject actor, string motivo, decimal? dinero = null)
    {
        if (hoja["registro"] is not JsonArray registro)
        {
            registro = [];
            hoja["registro"] = registro;
        }
        var movimiento = new JsonObject
        {
            ["id"] = NuevoId(),
            ["en"] = DateTime.UtcNow.ToString("O"),
            ["autor"] = autor,
            ["tipo"] = tipo,
            ["actor"] = actor,
            ["motivo"] = motivo,
        };
        if (dinero is not null) movimiento["dinero"] = dinero.Value;
        registro.Insert(0, movimiento);
        while (registro.Count > MaxRegistro) registro.RemoveAt(registro.Count - 1);
    }

    /// <summary>A ledger text the web wrote: trimmed, at most 200 characters; empty gives the default</summary>
    private static string TextoRegistro(string? texto, string porDefecto)
    {
        var limpio = (texto ?? "").Trim();
        if (limpio.Length > 200)
            throw new ArgumentException("The ledger text is too long.");
        return limpio.Length > 0 ? limpio : porDefecto;
    }

    private async Task<string> AutorAsync(long userId) =>
        await db.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => u.DisplayName).FirstOrDefaultAsync() ?? "";

    private static JsonObject Hoja(CrewSheetEntity row) => JsonNode.Parse(row.State) as JsonObject ?? new JsonObject();

    private static JsonObject Actor(CharacterEntity c) => new() { ["tipo"] = "pj", ["characterId"] = c.Id, ["nombre"] = c.Name };

    private static string NuevoId() => Guid.NewGuid().ToString("N")[..12];

    private static string? Cadena(JsonNode? n) => n is JsonValue v && v.TryGetValue<string>(out var s) ? s : null;

    /// <summary>
    /// A number of the sheet. A value parsed from JSON reads as decimal; one the server set itself (`lote["cantidad"] = 4`) keeps its
    /// C# type and only reads as that type, so the others are tried too
    /// </summary>
    private static decimal? Numero(JsonNode? n)
    {
        if (n is not JsonValue v) return null;
        if (v.TryGetValue<decimal>(out var d)) return d;
        if (v.TryGetValue<int>(out var i)) return i;
        if (v.TryGetValue<long>(out var l)) return l;
        return v.TryGetValue<double>(out var x) && double.IsFinite(x) ? (decimal)x : null;
    }

    private static string Importe(decimal? v) => v is null ? "-" : v.Value.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>
    /// What makes two open lots the same goods, as the web decides it (claveMercancia in src/pages/banda/estado.ts): the item (its
    /// price-table entry, or its inventory list and name), the conservation, the value and the situation, and the price when on sale
    /// </summary>
    private static string ClaveMercancia(JsonObject m)
    {
        var nombre = Cadena(m["nombre"]) ?? "";
        var item = m["deCatalogo"] is JsonObject catalogo && Numero(catalogo["id"]) is { } id
            ? $"{Cadena(catalogo["tipo"])}:{Importe(id)}"
            : $"{Cadena(m["lista"]) ?? "otra"}:{nombre}";
        var situacion = Cadena(m["situacion"]) ?? "";
        var precio = situacion == "en-venta" ? Importe(Numero(m["precio"])) : "-";
        return string.Join('|', item, nombre, Cadena(m["conservacion"]) ?? "nuevo", Importe(Numero(m["valor"])), situacion, precio);
    }

    private static int CantidadDe(JsonObject m) => (int)Math.Clamp(Numero(m["cantidad"]) ?? 1m, 1m, 999m);

    private static bool Abierto(JsonObject m) => Cadena(m["situacion"]) is "guardada" or "en-venta";

    /// <summary>The same actor (who brought a lot or put it on sale): the same character, or both the director</summary>
    private static bool MismoActor(JsonNode? a, JsonNode? b) =>
        a is JsonObject x && b is JsonObject y && Cadena(x["tipo"]) is { } tipo && tipo == Cadena(y["tipo"])
        && (tipo != "pj" || Numero(x["characterId"]) == Numero(y["characterId"]));

    /// <summary>
    /// Adds a lot to the store as the web does (juntarEnAlmacen in src/pages/banda/estado.ts): joined to the first open lot of the same
    /// goods with room for its units (up to 999), which keeps its id and its weeks on sale, adds the notes and keeps who brought it only
    /// when it is the same; otherwise at the end
    /// </summary>
    private static void JuntarEnAlmacen(List<JsonObject> lotes, JsonObject nuevo)
    {
        var clave = Abierto(nuevo) ? ClaveMercancia(nuevo) : null;
        var destino = clave is null ? null : lotes.FirstOrDefault(m => Abierto(m) && ClaveMercancia(m) == clave && CantidadDe(m) + CantidadDe(nuevo) <= 999);
        if (destino is null)
        {
            lotes.Add(nuevo);
            return;
        }
        destino["cantidad"] = CantidadDe(destino) + CantidadDe(nuevo);
        if (!MismoActor(destino["traidaPor"], nuevo["traidaPor"])) destino["traidaPor"] = null;
        var (puesta, otra) = (destino["puestaPor"], nuevo["puestaPor"]);
        destino["puestaPor"] = puesta is not null && otra is not null && !MismoActor(puesta, otra) ? null : (puesta ?? otra)?.DeepClone();
        destino["notas"] = string.Join(" · ", new[] { Cadena(destino["notas"]) ?? "", Cadena(nuevo["notas"]) ?? "" }.Where(t => t.Length > 0).Distinct());
    }

    /// <summary>
    /// The store's lots as detached copies, with the open lots of the same goods saved apart already joined, as the web shows them
    /// (normalizarBanda joins them in the same order): the first one keeps its id
    /// </summary>
    private static List<JsonObject> Almacen(JsonObject hoja)
    {
        var lotes = new List<JsonObject>();
        if (hoja["almacen"] is JsonArray almacen)
        {
            foreach (var m in almacen.OfType<JsonObject>())
                JuntarEnAlmacen(lotes, m.DeepClone().AsObject());
        }
        return lotes;
    }

    private static bool IsLostRace(DbUpdateException e) =>
        e is DbUpdateConcurrencyException || e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    /// <summary>A player only reaches a sheet the GM has shared with them; anything else is 403.</summary>
    private static void EnsureShared(CrewSheetEntity? row, long userId)
    {
        if (row is null || !row.SharedWith.Contains(userId))
            throw new UnauthorizedAccessException("The crew sheet is not shared with you.");
    }

    /// <summary>The text to store, after checking it is a JSON object that fits and can be written back.</summary>
    private static string ValidJson(JsonElement element, int maxLength, string name)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new ArgumentException($"{name} must be a JSON object.");
        var json = element.GetRawText();
        if (json.Length > maxLength)
            throw new ArgumentException($"{name} is too large.");
        // A lone UTF-16 surrogate (half an emoji, «\ud83d») parses but cannot be written back as JSON: stored, it would make
        // every later GET and 409 of this campaign fail. Writing it once here finds it before anything is saved.
        try
        {
            JsonSerializer.SerializeToUtf8Bytes(element);
        }
        catch (Exception e) when (e is JsonException or ArgumentException or InvalidOperationException)
        {
            throw new ArgumentException($"{name} contains invalid text.");
        }
        return json;
    }

    /// <summary>Not a member → 404 (the campaign is not revealed); returns the member's role.</summary>
    private async Task<string> RoleAsync(long campaignId, long userId) =>
        await db.CampaignMembers
            .Where(m => m.CampaignId == campaignId && m.UserId == userId)
            .Select(m => m.Role)
            .FirstOrDefaultAsync()
        ?? throw new KeyNotFoundException("Campaign not found.");

    /// <summary>Not a member → 404 (the campaign is not revealed); a member who is not the GM → 403.</summary>
    private async Task EnsureGmAsync(long campaignId, long userId)
    {
        if (await RoleAsync(campaignId, userId) != "gm")
            throw new UnauthorizedAccessException("Only the GM can perform this action.");
    }

    /// <summary>The save was refused: carry the stored sheet (or the empty one when there is none) back to the client, as it may read it.</summary>
    private static CrewSheetSaveResult Conflict(CrewSheetEntity? stored, bool forGm) =>
        new(false, stored is null ? EmptyResponse() : MapToResponse(stored, forGm));

    private static CrewSheetResponse EmptyResponse() => new()
    {
        State = ParseJson("{}"),
        GmState = ParseJson("{}"),
        SharedWith = [],
        Version = 0,
        UpdatedAt = null,
    };

    private static CrewSheetResponse MapToResponse(CrewSheetEntity c, bool forGm) => new()
    {
        State = ParseJson(c.State),
        GmState = forGm ? ParseJson(c.GmState) : null,
        SharedWith = forGm ? [.. c.SharedWith] : null,
        Version = c.Version,
        // A row created by sharing before the first save holds no sheet yet
        UpdatedAt = c.Version == 0 ? null : c.UpdatedAt,
    };

    /// <summary>The root element outlives the parsed document: <c>Clone</c> detaches it so the document can be disposed.</summary>
    private static JsonElement ParseJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
