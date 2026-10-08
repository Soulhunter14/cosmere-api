using System.Text.Json;
using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.GmScreen.In;
using Messages.GmScreen.Out;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Services.GmScreen;

public class GmScreenService(CosmereContext db) : IGmScreenService
{
    /// <summary>Largest accepted document, in characters of its JSON text.</summary>
    public const int MaxStateLength = 1_000_000;

    public async Task<GmScreenResponse> GetAsync(long campaignId, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        var row = await db.GmScreens.AsNoTracking().FirstOrDefaultAsync(g => g.CampaignId == campaignId);
        return row is null ? EmptyResponse() : MapToResponse(row);
    }

    public async Task<GmScreenSaveResult> SaveAsync(long campaignId, SaveGmScreenRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        if (request.State.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("State must be a JSON object.");
        var json = request.State.GetRawText();
        if (json.Length > MaxStateLength)
            throw new ArgumentException("State is too large.");
        // A lone UTF-16 surrogate (half an emoji, «\ud83d») parses but cannot be written back as JSON: stored, it would make
        // every later GET and 409 of this campaign fail. Writing it once here finds it before anything is saved.
        try
        {
            JsonSerializer.SerializeToUtf8Bytes(request.State);
        }
        catch (Exception e) when (e is JsonException or ArgumentException or InvalidOperationException)
        {
            throw new ArgumentException("State contains invalid text.");
        }

        var row = await db.GmScreens.FirstOrDefaultAsync(g => g.CampaignId == campaignId);
        if (row is null)
        {
            // First save: the client must have read the empty document, which is version 0.
            if (request.Version != 0)
                return Conflict(stored: null);

            row = new GmScreenEntity { CampaignId = campaignId, State = json, Version = 1, UpdatedAt = DateTime.UtcNow };
            db.GmScreens.Add(row);
        }
        else
        {
            if (row.Version != request.Version)
                return Conflict(row);

            row.State = json;
            row.UpdatedAt = DateTime.UtcNow;
            // Version is a concurrency token: EF adds `WHERE "Version" = <the version read>` to the UPDATE.
            row.Version++;
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException e) when (e is DbUpdateConcurrencyException || e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Lost a race, and nothing of ours was written: another save took the version first, or two first saves collided
            // on the primary key. Answer with what is stored now, as for any other conflict. Any other failure (the campaign
            // deleted meanwhile, a timeout) is not a conflict and goes up as an error.
            db.ChangeTracker.Clear();
            var stored = await db.GmScreens.AsNoTracking().FirstOrDefaultAsync(g => g.CampaignId == campaignId);
            return Conflict(stored);
        }

        return new GmScreenSaveResult(true, MapToResponse(row));
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

    /// <summary>The save was refused: carry the stored document (or the empty one when there is none) back to the client.</summary>
    private static GmScreenSaveResult Conflict(GmScreenEntity? stored) =>
        new(false, stored is null ? EmptyResponse() : MapToResponse(stored));

    private static GmScreenResponse EmptyResponse() => new()
    {
        State = ParseState("{}"),
        Version = 0,
        UpdatedAt = null,
    };

    private static GmScreenResponse MapToResponse(GmScreenEntity g) => new()
    {
        State = ParseState(g.State),
        Version = g.Version,
        UpdatedAt = g.UpdatedAt,
    };

    /// <summary>The root element outlives the parsed document: <c>Clone</c> detaches it so the document can be disposed.</summary>
    private static JsonElement ParseState(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
