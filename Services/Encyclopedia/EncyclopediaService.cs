using System.Text;
using System.Text.RegularExpressions;
using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.Encyclopedia.In;
using Messages.Encyclopedia.Out;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Services.Encyclopedia;

/// <summary>
/// The originals of the encyclopedia (book text and mini-PDF of each entry), shared by every campaign of a world. Always through a
/// campaign, which gives the world: any member reads them, only its GM uploads or removes them.
/// </summary>
public partial class EncyclopediaService(CosmereContext db) : IEncyclopediaService
{
    /// <summary>Largest accepted mini-PDF: nginx takes 4 MB per request (`client_max_body_size 4m`), multipart included.</summary>
    public const int MaxPdfBytes = 3_900_000;
    /// <summary>Largest accepted text, in characters (the longest entry, a heroic path, is about 30 000).</summary>
    public const int MaxMdLength = 300_000;
    public const int MaxTitleLength = 200;

    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();

    public async Task<List<EncyclopediaOriginalSummaryResponse>> ListAsync(long campaignId, long userId)
    {
        var world = await GetWorldAsync(campaignId, userId);
        return await db.EncyclopediaOriginals.AsNoTracking()
            .Where(o => o.World == world)
            .OrderBy(o => o.Section).ThenBy(o => o.Key)
            .Select(o => new EncyclopediaOriginalSummaryResponse
            {
                Section = o.Section,
                Key = o.Key,
                Title = o.Title,
                TextLength = o.Md == null ? null : o.Md.Length,
                PdfBytes = o.Pdf == null ? null : o.Pdf.Length,
                UpdatedAt = o.UpdatedAt,
            })
            .ToListAsync();
    }

    public async Task<EncyclopediaTextResponse> GetTextAsync(long campaignId, string section, string key, long userId)
    {
        CheckSlugs(section, key);
        var world = await GetWorldAsync(campaignId, userId);
        return await db.EncyclopediaOriginals.AsNoTracking()
            .Where(o => o.World == world && o.Section == section && o.Key == key && o.Md != null)
            .Select(o => new EncyclopediaTextResponse { Section = o.Section, Key = o.Key, Title = o.Title, Md = o.Md!, UpdatedAt = o.UpdatedAt })
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Text not found.");
    }

    public async Task<(byte[] Data, string Title)> GetPdfAsync(long campaignId, string section, string key, long userId)
    {
        CheckSlugs(section, key);
        var world = await GetWorldAsync(campaignId, userId);
        var row = await db.EncyclopediaOriginals.AsNoTracking()
            .Where(o => o.World == world && o.Section == section && o.Key == key && o.Pdf != null)
            .Select(o => new { o.Pdf, o.Title })
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("PDF not found.");
        return (row.Pdf!, row.Title);
    }

    public async Task<EncyclopediaOriginalSummaryResponse> SaveTextAsync(long campaignId, string section, string key, SaveEncyclopediaTextRequest request, long userId)
    {
        CheckSlugs(section, key);
        var world = await GetWorldAsync(campaignId, userId, requireGm: true);
        var title = CheckTitle(request.Title);
        var md = request.Md ?? string.Empty;
        if (md.Trim().Length == 0) throw new ArgumentException("The text is empty.");
        if (md.Length > MaxMdLength) throw new ArgumentException("The text is too large.");
        CheckUtf8(md);

        return await UpsertAsync(world, section, key, title, o => o.Md = md);
    }

    public async Task<EncyclopediaOriginalSummaryResponse> SavePdfAsync(long campaignId, string section, string key, string title, byte[] data, long userId)
    {
        CheckSlugs(section, key);
        var world = await GetWorldAsync(campaignId, userId, requireGm: true);
        var t = CheckTitle(title);
        if (data.Length == 0 || data.Length > MaxPdfBytes)
            throw new ArgumentException($"The PDF must weigh between 1 byte and {MaxPdfBytes / 1_000_000.0:0.#} MB.");
        // The type by its first bytes («%PDF-»): the client's Content-Type is not trusted
        if (data.Length < 5 || data[0] != '%' || data[1] != 'P' || data[2] != 'D' || data[3] != 'F' || data[4] != '-')
            throw new ArgumentException("The file is not a PDF.");

        return await UpsertAsync(world, section, key, t, o => o.Pdf = data);
    }

    public Task DeleteTextAsync(long campaignId, string section, string key, long userId) =>
        RemovePartAsync(campaignId, section, key, userId, o => o.Md = null);

    public Task DeletePdfAsync(long campaignId, string section, string key, long userId) =>
        RemovePartAsync(campaignId, section, key, userId, o => o.Pdf = null);

    // ── Helpers ──

    private async Task<EncyclopediaOriginalSummaryResponse> UpsertAsync(string world, string section, string key, string title, Action<EncyclopediaOriginalEntity> set)
    {
        var row = await db.EncyclopediaOriginals.FirstOrDefaultAsync(o => o.World == world && o.Section == section && o.Key == key);
        if (row is null)
        {
            row = new EncyclopediaOriginalEntity { World = world, Section = section, Key = key, Title = title };
            db.EncyclopediaOriginals.Add(row);
        }
        row.Title = title;
        set(row);
        row.UpdatedAt = DateTime.UtcNow;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Two first uploads of the same entry at once: the other one was stored first; this one goes on top of it
            db.ChangeTracker.Clear();
            row = await db.EncyclopediaOriginals.FirstAsync(o => o.World == world && o.Section == section && o.Key == key);
            row.Title = title;
            set(row);
            row.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        return new EncyclopediaOriginalSummaryResponse
        {
            Section = row.Section,
            Key = row.Key,
            Title = row.Title,
            TextLength = row.Md?.Length,
            PdfBytes = row.Pdf?.Length,
            UpdatedAt = row.UpdatedAt,
        };
    }

    /// <summary>Removes the text or the PDF of an entry; the row goes when neither is left.</summary>
    private async Task RemovePartAsync(long campaignId, string section, string key, long userId, Action<EncyclopediaOriginalEntity> clear)
    {
        CheckSlugs(section, key);
        var world = await GetWorldAsync(campaignId, userId, requireGm: true);
        var row = await db.EncyclopediaOriginals.FirstOrDefaultAsync(o => o.World == world && o.Section == section && o.Key == key);
        if (row is null) return;
        clear(row);
        if (row.Md is null && row.Pdf is null) db.EncyclopediaOriginals.Remove(row);
        else row.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private static void CheckSlugs(string section, string key)
    {
        if (section.Length > 64 || !SlugRegex().IsMatch(section)) throw new ArgumentException("Unknown section.");
        if (key.Length > 64 || !SlugRegex().IsMatch(key)) throw new ArgumentException("Unknown entry.");
    }

    private static string CheckTitle(string? title)
    {
        var t = (title ?? string.Empty).Trim();
        if (t.Length == 0 || t.Length > MaxTitleLength)
            throw new ArgumentException($"The title is required and has at most {MaxTitleLength} characters.");
        CheckUtf8(t);
        return t;
    }

    /// <summary>Strict UTF-8: it refuses a lone UTF-16 surrogate (half an emoji), which PostgreSQL could not store.</summary>
    private static void CheckUtf8(string s)
    {
        try { StrictUtf8.GetByteCount(s); }
        catch (EncoderFallbackException) { throw new ArgumentException("The text contains invalid characters."); }
    }

    /// <summary>
    /// The world of the campaign; the caller must be a member (not a member → 404, the campaign is not revealed) and, to write,
    /// its GM (403): the originals are shared by every campaign of the world.
    /// </summary>
    private async Task<string> GetWorldAsync(long campaignId, long userId, bool requireGm = false)
    {
        var member = await db.CampaignMembers.AsNoTracking()
            .Where(m => m.CampaignId == campaignId && m.UserId == userId)
            .Select(m => new { m.Role, m.Campaign.World })
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Campaign not found.");
        if (requireGm && member.Role != "gm") throw new UnauthorizedAccessException("Only the GM can perform this action.");
        return member.World;
    }
}
