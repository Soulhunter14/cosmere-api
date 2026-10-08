using System.Text;
using Infrastructure.Data;
using Messages.Book.In;
using Messages.Book.Out;
using Messages.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Services.Book;

public class BookService(CosmereContext db) : IBookService
{
    /// <summary>Largest accepted chapter, in characters of its Markdown (the longest chapter of El legado is about 120 000).</summary>
    public const int MaxMdLength = 500_000;
    public const int MaxTitleLength = 200;
    public const int MaxNumber = 99;

    /// <summary>Strict UTF-8: it refuses a lone UTF-16 surrogate (half an emoji), which PostgreSQL could not store.</summary>
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public async Task<List<BookChapterSummaryResponse>> ListAsync(long campaignId, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        return await db.BookChapters.AsNoTracking()
            .Where(c => c.CampaignId == campaignId)
            .OrderBy(c => c.Number)
            .Select(c => new BookChapterSummaryResponse { Number = c.Number, Title = c.Title, Length = c.Md.Length, UpdatedAt = c.UpdatedAt })
            .ToListAsync();
    }

    public async Task<BookChapterResponse> GetAsync(long campaignId, int number, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        var row = await db.BookChapters.AsNoTracking().FirstOrDefaultAsync(c => c.CampaignId == campaignId && c.Number == number)
            ?? throw new KeyNotFoundException("Chapter not found.");
        return MapToResponse(row);
    }

    public async Task<BookChapterResponse> SaveAsync(long campaignId, int number, SaveBookChapterRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        if (number < 1 || number > MaxNumber)
            throw new ArgumentException($"The chapter number must be between 1 and {MaxNumber}.");
        var title = (request.Title ?? string.Empty).Trim();
        var md = request.Md ?? string.Empty;
        if (title.Length == 0 || title.Length > MaxTitleLength)
            throw new ArgumentException($"The title is required and has at most {MaxTitleLength} characters.");
        if (md.Trim().Length == 0)
            throw new ArgumentException("The chapter is empty.");
        if (md.Length > MaxMdLength)
            throw new ArgumentException("The chapter is too large.");
        try
        {
            StrictUtf8.GetByteCount(title);
            StrictUtf8.GetByteCount(md);
        }
        catch (EncoderFallbackException)
        {
            throw new ArgumentException("The chapter contains invalid text.");
        }

        var row = await db.BookChapters.FirstOrDefaultAsync(c => c.CampaignId == campaignId && c.Number == number);
        if (row is null)
        {
            row = new BookChapterEntity { CampaignId = campaignId, Number = number, Title = title, Md = md, UpdatedAt = DateTime.UtcNow };
            db.BookChapters.Add(row);
        }
        else
        {
            row.Title = title;
            row.Md = md;
            row.UpdatedAt = DateTime.UtcNow;
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Two first uploads of the same chapter at once: the other one was stored first; this one replaces it
            db.ChangeTracker.Clear();
            row = await db.BookChapters.FirstAsync(c => c.CampaignId == campaignId && c.Number == number);
            row.Title = title;
            row.Md = md;
            row.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        return MapToResponse(row);
    }

    public async Task DeleteAsync(long campaignId, int number, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        await db.BookChapters.Where(c => c.CampaignId == campaignId && c.Number == number).ExecuteDeleteAsync();
    }

    /// <summary>Not a member → 404 (the campaign is not revealed); a member who is not the GM → 403.</summary>
    private async Task EnsureGmAsync(long campaignId, long userId)
    {
        var role = await db.CampaignMembers
            .Where(m => m.CampaignId == campaignId && m.UserId == userId)
            .Select(m => m.Role)
            .FirstOrDefaultAsync();

        if (role is null)
            throw new KeyNotFoundException("Campaign not found.");
        if (role != "gm")
            throw new UnauthorizedAccessException("Only the GM can perform this action.");
    }

    private static BookChapterResponse MapToResponse(BookChapterEntity c) => new()
    {
        Number = c.Number,
        Title = c.Title,
        Md = c.Md,
        UpdatedAt = c.UpdatedAt,
    };
}
