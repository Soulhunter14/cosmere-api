using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.GlobalNpcs.In;
using Messages.GlobalNpcs.Out;
using Messages.Worlds;
using Microsoft.EntityFrameworkCore;

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

    public async Task<GlobalNpcResponse> CreateAsync(GlobalNpcRequest request, long? campaignId, long userId)
    {
        var (world, era) = await GetScopeAsync(campaignId, userId, requireGm: true);
        var entity = FromRequest(request, world);
        entity.Era = era;
        db.GlobalNpcs.Add(entity);
        await db.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<GlobalNpcResponse> UpdateAsync(long id, GlobalNpcRequest request, long? campaignId, long userId)
    {
        var entity = await FindInWorldAsync(id, campaignId, userId);
        Apply(entity, request);
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return (await WithBookImagesAsync([Map(entity)]))[0];
    }

    public async Task DeleteAsync(long id, long? campaignId, long userId)
    {
        var entity = await FindInWorldAsync(id, campaignId, userId);
        db.GlobalNpcs.Remove(entity);
        await db.SaveChangesAsync();
    }

    // ── Book images (AddGlobalNpcImages): the stat block, the tactics box and the illustration as the book prints them ──
    // Only the GM of a campaign of the adversary's world reads or writes them, always through that campaign: without one the
    // catalog falls back to the old Stormlight path, which checks no GM.

    public async Task<(byte[] Data, string ContentType)> GetBookImageAsync(long id, string kind, long? campaignId, long userId)
    {
        CheckKind(kind);
        await FindInWorldAsync(id, RequireCampaign(campaignId), userId);
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
        var type = ImageType(data) ?? throw new ArgumentException("The image must be WebP, PNG or JPEG.");

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

    /// <summary>The type of an image by its first bytes (the client's Content-Type is not trusted); null = not WebP, PNG or JPEG.</summary>
    private static string? ImageType(byte[] d) =>
        d.Length >= 12 && d[0] == 'R' && d[1] == 'I' && d[2] == 'F' && d[3] == 'F' && d[8] == 'W' && d[9] == 'E' && d[10] == 'B' && d[11] == 'P' ? "image/webp"
        : d.Length >= 8 && d[0] == 0x89 && d[1] == 'P' && d[2] == 'N' && d[3] == 'G' ? "image/png"
        : d.Length >= 3 && d[0] == 0xFF && d[1] == 0xD8 && d[2] == 0xFF ? "image/jpeg"
        : null;

    /// <summary>
    /// World and era (catalog number, <see cref="EraIds.Numero"/>) of the operation: the campaign's (the caller must belong to
    /// the campaign and, to write, be its GM, as in the catalog) or, without a campaign, Stormlight and no era (clients that predate
    /// worlds). Neither comes from the request body.
    /// </summary>
    private async Task<(string World, short? Era)> GetScopeAsync(long? campaignId, long userId, bool requireGm = false)
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
        if (role is null) throw new UnauthorizedAccessException("You are not a member of this campaign.");
        // The adversaries are shared by every campaign of the world: a player of one campaign must not change them for all
        if (requireGm && role != "gm") throw new UnauthorizedAccessException("Only the GM can perform this action.");

        return (campaign.World, EraIds.Numero(campaign.Era));
    }

    /// <summary>The NPC to write to (GM only); 404 if it does not exist or belongs to a different world than the operation's.</summary>
    private async Task<GlobalNpcEntity> FindInWorldAsync(long id, long? campaignId, long userId)
    {
        var (world, _) = await GetScopeAsync(campaignId, userId, requireGm: true);
        var entity = await db.GlobalNpcs.FindAsync(id);
        if (entity is null || entity.World != world) throw new KeyNotFoundException("Global NPC not found.");
        return entity;
    }

    private static GlobalNpcEntity FromRequest(GlobalNpcRequest r, string world) => new()
    {
        World = world,
        Name = r.Name, Source = r.Source, Tipo = r.Tipo, Ascendencia = r.Ascendencia, Level = r.Level,
        Fuerza = r.Fuerza, Velocidad = r.Velocidad, Intelecto = r.Intelecto,
        Voluntad = r.Voluntad, Discernimiento = r.Discernimiento, Presencia = r.Presencia,
        MaxHealth = r.MaxHealth, MaxConcentration = r.MaxConcentration, MaxInvestiture = r.MaxInvestiture,
        Agilidad = r.Agilidad, ArmasLigeras = r.ArmasLigeras, ArmasPesadas = r.ArmasPesadas,
        Atletismo = r.Atletismo, Hurto = r.Hurto, Sigilo = r.Sigilo,
        Deduccion = r.Deduccion, Disciplina = r.Disciplina, Intimidacion = r.Intimidacion,
        Manufactura = r.Manufactura, Medicina = r.Medicina, Conocimiento = r.Conocimiento,
        Engano = r.Engano, Liderazgo = r.Liderazgo, Percepcion = r.Percepcion,
        Perspicacia = r.Perspicacia, Persuasion = r.Persuasion, Supervivencia = r.Supervivencia,
        Talentos = r.Talentos, Apariencia = r.Apariencia, Notas = r.Notas,
    };

    private static void Apply(GlobalNpcEntity e, GlobalNpcRequest r)
    {
        e.Name = r.Name; e.Source = r.Source; e.Tipo = r.Tipo; e.Ascendencia = r.Ascendencia; e.Level = r.Level;
        e.Fuerza = r.Fuerza; e.Velocidad = r.Velocidad; e.Intelecto = r.Intelecto;
        e.Voluntad = r.Voluntad; e.Discernimiento = r.Discernimiento; e.Presencia = r.Presencia;
        e.MaxHealth = r.MaxHealth; e.MaxConcentration = r.MaxConcentration; e.MaxInvestiture = r.MaxInvestiture;
        e.Agilidad = r.Agilidad; e.ArmasLigeras = r.ArmasLigeras; e.ArmasPesadas = r.ArmasPesadas;
        e.Atletismo = r.Atletismo; e.Hurto = r.Hurto; e.Sigilo = r.Sigilo;
        e.Deduccion = r.Deduccion; e.Disciplina = r.Disciplina; e.Intimidacion = r.Intimidacion;
        e.Manufactura = r.Manufactura; e.Medicina = r.Medicina; e.Conocimiento = r.Conocimiento;
        e.Engano = r.Engano; e.Liderazgo = r.Liderazgo; e.Percepcion = r.Percepcion;
        e.Perspicacia = r.Perspicacia; e.Persuasion = r.Persuasion; e.Supervivencia = r.Supervivencia;
        e.Talentos = r.Talentos; e.Apariencia = r.Apariencia; e.Notas = r.Notas;
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
