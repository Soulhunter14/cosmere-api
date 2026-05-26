using System.Text.Json;
using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.Diary.Out;
using Microsoft.EntityFrameworkCore;

namespace Services.Diary;

public class DiaryService(CosmereContext db) : IDiaryService
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<List<DiaryEntryResponse>> GetEntriesAsync(long campaignId, long userId)
    {
        await EnsureMemberAsync(campaignId, userId);

        var entries = await db.DiaryEntries
            .Where(e => e.CampaignId == campaignId)
            .OrderBy(e => e.Number)
            .ToListAsync();

        return entries.Select(MapToResponse).ToList();
    }

    public async Task<DiaryEntryResponse> GetEntryAsync(long entryId, long campaignId, long userId)
    {
        await EnsureMemberAsync(campaignId, userId);

        var entry = await db.DiaryEntries
            .FirstOrDefaultAsync(e => e.Id == entryId && e.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Diary entry not found.");

        return MapToResponse(entry);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task EnsureMemberAsync(long campaignId, long userId)
    {
        var isMember = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId);
        if (!isMember) throw new KeyNotFoundException("Campaign not found.");
    }

    private static List<DiaryMentionDto> ParseMentions(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return [];
        try { return JsonSerializer.Deserialize<List<DiaryMentionDto>>(raw, _jsonOptions) ?? []; }
        catch { return []; }
    }

    private static DiaryEntryResponse MapToResponse(DiaryEntryEntity e) => new()
    {
        Id           = e.Id,
        CampaignId   = e.CampaignId,
        Number       = e.Number,
        Title        = e.Title,
        Slug         = e.Slug,
        Preview      = e.Preview,
        Body         = e.Body,
        Participants = e.Participants,
        Mentions     = ParseMentions(e.MentionsJson),
        CreatedAt    = e.CreatedAt,
        UpdatedAt    = e.UpdatedAt,
    };
}
