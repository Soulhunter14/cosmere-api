using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.GlobalNpcs.Out;
using Messages.Worlds;
using Microsoft.EntityFrameworkCore;
using Services.Images;

namespace Services.GlobalNpcs;

public class GlobalNpcService(CosmereContext db) : IGlobalNpcService
{
    /// <summary>Largest accepted book image (a stat block cut at 200 dpi weighs 150-600 KB as WebP).</summary>
    public const int MaxImageBytes = 3 * 1024 * 1024;

    public async Task<List<GlobalNpcResponse>> GetAllAsync(long? campaignId, long userId)
    {
        // The campaign's world and, in an era campaign, the adversaries of its era plus those of both eras (Era null)
        var (world, era) = await GetScopeAsync(campaignId, userId);
        var npcs = await db.GlobalNpcs
            .Where(n => n.World == world && (era == null || n.Era == null || n.Era == era))
            .OrderBy(n => n.Name).Select(n => Map(n)).ToListAsync();
        return await WithBookImagesAsync(npcs);
    }

    public async Task<GlobalNpcResponse> GetByIdAsync(long id) =>
        (await WithBookImagesAsync([Map(await db.GlobalNpcs.FindAsync(id) ?? throw new KeyNotFoundException("Global NPC not found."))]))[0];

    // ── Book images (AddGlobalNpcImages): the stat block, the tactics box and the illustration as the book prints them ──
    // The only writes of the catalog: its adversaries come in migrations. Always through a campaign of the adversary's world:
    // without one the catalog falls back to the old Stormlight path, which checks no membership and only serves to read the list.
    // Only the GM of that campaign writes them or reads the stat block and the tactics box; the illustration («retrato») may also
    // be read by any member of the campaign, so the GM can show it to the players at the table («La mesa»).

    public async Task<(byte[] Data, string ContentType)> GetBookImageAsync(long id, string kind, long? campaignId, long userId)
    {
        CheckKind(kind);
        // «ficha» and «tacticas» are the GM's; «retrato» is public inside the campaign, where a non-member is told nothing (404)
        var isPortrait = kind == GlobalNpcImageKinds.Retrato;
        await FindInWorldAsync(id, RequireCampaign(campaignId), userId, requireGm: !isPortrait);
        var image = await db.GlobalNpcImages.AsNoTracking()
            .Where(i => i.GlobalNpcId == id && i.Kind == kind)
            .Select(i => new { i.Data, i.ContentType })
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Image not found.");
        return (image.Data, image.ContentType);
    }

    public async Task<GlobalNpcResponse> SaveBookImageAsync(long id, string kind, byte[] data, long? campaignId, long userId)
    {
        CheckKind(kind);
        var npc = await FindInWorldAsync(id, RequireCampaign(campaignId), userId);
        if (data.Length == 0 || data.Length > MaxImageBytes) throw new ArgumentException($"The image must weigh between 1 byte and {MaxImageBytes / 1024 / 1024} MB.");
        var type = ImageType.Detect(data) ?? throw new ArgumentException("The image must be WebP, PNG or JPEG.");

        var row = await db.GlobalNpcImages.FirstOrDefaultAsync(i => i.GlobalNpcId == id && i.Kind == kind);
        if (row is null)
        {
            db.GlobalNpcImages.Add(new GlobalNpcImageEntity { GlobalNpcId = id, Kind = kind, ContentType = type, Data = data });
        }
        else
        {
            row.Data = data;
            row.ContentType = type;
            row.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
        return (await WithBookImagesAsync([Map(npc)]))[0];
    }

    public async Task DeleteBookImageAsync(long id, string kind, long? campaignId, long userId)
    {
        CheckKind(kind);
        await FindInWorldAsync(id, RequireCampaign(campaignId), userId);
        var row = await db.GlobalNpcImages.FirstOrDefaultAsync(i => i.GlobalNpcId == id && i.Kind == kind)
            ?? throw new KeyNotFoundException("Image not found.");
        db.GlobalNpcImages.Remove(row);
        await db.SaveChangesAsync();
    }

    /// <summary>Which book images each adversary has: one query for the whole list, without loading the images.</summary>
    private async Task<List<GlobalNpcResponse>> WithBookImagesAsync(List<GlobalNpcResponse> npcs)
    {
        var ids = npcs.Select(n => n.Id).ToList();
        var rows = await db.GlobalNpcImages.AsNoTracking()
            .Where(i => ids.Contains(i.GlobalNpcId))
            .Select(i => new { i.GlobalNpcId, i.Kind })
            .ToListAsync();
        var kinds = rows.ToLookup(r => r.GlobalNpcId, r => r.Kind);
        foreach (var n in npcs) n.BookImages = [.. GlobalNpcImageKinds.All.Where(k => kinds[n.Id].Contains(k))];
        return npcs;
    }

    private static void CheckKind(string kind)
    {
        if (!GlobalNpcImageKinds.All.Contains(kind)) throw new ArgumentException("Unknown image kind.");
    }

    private static long RequireCampaign(long? campaignId) => campaignId ?? throw new ArgumentException("campaignId is required.");

    /// <summary>
    /// World and era (catalog number, <see cref="EraIds.Numero"/>) of the operation: the campaign's (the caller must belong to
    /// the campaign and, for the book images, be its GM) or, without a campaign, Stormlight and no era (clients that predate
    /// worlds). Neither comes from the request. A non-member is refused with 403, or with 404 when
    /// <paramref name="hideFromNonMembers"/> (the campaign is not revealed).
    /// </summary>
    private async Task<(string World, short? Era)> GetScopeAsync(long? campaignId, long userId, bool requireGm = false, bool hideFromNonMembers = false)
    {
        if (campaignId is null) return (WorldIds.Stormlight, null);

        var campaign = await db.Campaigns.AsNoTracking()
            .Where(c => c.Id == campaignId)
            .Select(c => new { c.World, c.Era })
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Campaign not found.");

        var role = await db.CampaignMembers
            .Where(m => m.CampaignId == campaignId && m.UserId == userId)
            .Select(m => m.Role)
            .FirstOrDefaultAsync();
        if (role is null)
        {
            if (hideFromNonMembers) throw new KeyNotFoundException("Campaign not found.");
            throw new UnauthorizedAccessException("You are not a member of this campaign.");
        }
        // The adversaries are shared by every campaign of the world: a player of one campaign must not change them for all
        if (requireGm && role != "gm") throw new UnauthorizedAccessException("Only the GM can perform this action.");

        return (campaign.World, EraIds.Numero(campaign.Era));
    }

    /// <summary>
    /// The NPC of a book-image operation; 404 if it does not exist or belongs to a different world than the operation's. GM only,
    /// unless <paramref name="requireGm"/> is false: then any member of the campaign, and a non-member gets 404, not 403.
    /// </summary>
    private async Task<GlobalNpcEntity> FindInWorldAsync(long id, long? campaignId, long userId, bool requireGm = true)
    {
        var (world, _) = await GetScopeAsync(campaignId, userId, requireGm: requireGm, hideFromNonMembers: !requireGm);
        var entity = await db.GlobalNpcs.FindAsync(id);
        if (entity is null || entity.World != world) throw new KeyNotFoundException("Global NPC not found.");
        return entity;
    }

    private static GlobalNpcResponse Map(GlobalNpcEntity e) => new()
    {
        Id = e.Id, Name = e.Name, Source = e.Source, Tipo = e.Tipo, Ascendencia = e.Ascendencia, Level = e.Level,
        Fuerza = e.Fuerza, Velocidad = e.Velocidad, Intelecto = e.Intelecto,
        Voluntad = e.Voluntad, Discernimiento = e.Discernimiento, Presencia = e.Presencia,
        MaxHealth = e.MaxHealth, MaxConcentration = e.MaxConcentration, MaxInvestiture = e.MaxInvestiture,
        Agilidad = e.Agilidad, ArmasLigeras = e.ArmasLigeras, ArmasPesadas = e.ArmasPesadas,
        Atletismo = e.Atletismo, Hurto = e.Hurto, Sigilo = e.Sigilo,
        Deduccion = e.Deduccion, Disciplina = e.Disciplina, Intimidacion = e.Intimidacion,
        Manufactura = e.Manufactura, Medicina = e.Medicina, Conocimiento = e.Conocimiento,
        Engano = e.Engano, Liderazgo = e.Liderazgo, Percepcion = e.Percepcion,
        Perspicacia = e.Perspicacia, Persuasion = e.Persuasion, Supervivencia = e.Supervivencia,
        Talentos = e.Talentos, Apariencia = e.Apariencia, Notas = e.Notas,
        ImageUrl = e.ImageUrl, CreatedAt = e.CreatedAt, UpdatedAt = e.UpdatedAt,
        World = e.World, Era = e.Era,
    };
}
