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

    public async Task<NoteResponse> CreateNoteAsync(long campaignId, CreateNoteRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        var targetIsMember = await db.CampaignMembers
            .AnyAsync(m => m.CampaignId == campaignId && m.UserId == request.ToUserId);
        if (!targetIsMember) throw new KeyNotFoundException("Target player not found in campaign.");

        var note = new NoteEntity
        {
            CampaignId = campaignId,
            FromUserId = userId,
            ToUserId = request.ToUserId,
            Content = request.Content,
        };

        db.Notes.Add(note);
        await db.SaveChangesAsync();

        await db.Entry(note).Reference(n => n.FromUser).LoadAsync();
        await db.Entry(note).Reference(n => n.ToUser).LoadAsync();

        return MapToResponse(note);
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
