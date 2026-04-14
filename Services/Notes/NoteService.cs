using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.Notes.In;
using Messages.Notes.Out;
using Microsoft.EntityFrameworkCore;

namespace Services.Notes;

public class NoteService(CosmereContext db) : INoteService
{
    public async Task<List<NoteResponse>> GetNotesAsync(long campaignId, long userId)
    {
        var isMember = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId);
        if (!isMember) throw new KeyNotFoundException("Campaign not found.");

        var isGm = await IsGmAsync(campaignId, userId);

        var notes = isGm
            ? await db.Notes
                .Include(n => n.FromUser)
                .Include(n => n.ToUser)
                .Where(n => n.CampaignId == campaignId && n.FromUserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync()
            : await db.Notes
                .Include(n => n.FromUser)
                .Include(n => n.ToUser)
                .Where(n => n.CampaignId == campaignId && n.ToUserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

        return notes.Select(MapToResponse).ToList();
    }

    public async Task<List<NoteResponse>> CreateNoteAsync(long campaignId, CreateNoteRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        if (request.ToUserIds.Count == 0)
            throw new ArgumentException("At least one recipient is required.");

        var memberIds = await db.CampaignMembers
            .Where(m => m.CampaignId == campaignId && request.ToUserIds.Contains(m.UserId))
            .Select(m => m.UserId)
            .ToListAsync();

        var missing = request.ToUserIds.Except(memberIds).ToList();
        if (missing.Count > 0)
            throw new KeyNotFoundException("One or more target players not found in campaign.");

        var notes = request.ToUserIds.Select(toId => new NoteEntity
        {
            CampaignId = campaignId,
            FromUserId = userId,
            ToUserId = toId,
            Content = request.Content,
        }).ToList();

        db.Notes.AddRange(notes);
        await db.SaveChangesAsync();

        var noteIds = notes.Select(n => n.Id).ToList();
        var saved = await db.Notes
            .Include(n => n.FromUser)
            .Include(n => n.ToUser)
            .Where(n => noteIds.Contains(n.Id))
            .ToListAsync();

        return saved.Select(MapToResponse).ToList();
    }

    public async Task MarkAsReadAsync(long noteId, long campaignId, long userId)
    {
        var note = await db.Notes
            .FirstOrDefaultAsync(n => n.Id == noteId && n.CampaignId == campaignId && n.ToUserId == userId)
            ?? throw new KeyNotFoundException("Note not found.");

        note.IsRead = true;
        await db.SaveChangesAsync();
    }

    private async Task<bool> IsGmAsync(long campaignId, long userId) =>
        await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId && m.Role == "gm");

    private async Task EnsureGmAsync(long campaignId, long userId)
    {
        if (!await IsGmAsync(campaignId, userId))
            throw new UnauthorizedAccessException("Only the GM can perform this action.");
    }

    private static NoteResponse MapToResponse(NoteEntity n) => new()
    {
        Id = n.Id,
        CampaignId = n.CampaignId,
        FromUserId = n.FromUserId,
        ToUserId = n.ToUserId,
        FromDisplayName = n.FromUser?.DisplayName ?? string.Empty,
        ToDisplayName = n.ToUser?.DisplayName ?? string.Empty,
        Content = n.Content,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt,
    };
}
