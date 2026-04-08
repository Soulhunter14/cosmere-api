using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.NpcNotes.In;
using Messages.NpcNotes.Out;
using Microsoft.EntityFrameworkCore;

namespace Services.NpcNotes;

public class NpcNoteService(CosmereContext db) : INpcNoteService
{
    public async Task<List<NpcNoteResponse>> GetNpcNotesAsync(long campaignId, long userId)
    {
        var isMember = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId);
        if (!isMember) throw new KeyNotFoundException("Campaign not found.");

        return await db.NpcNotes
            .Include(n => n.Author)
            .Where(n => n.CampaignId == campaignId && (n.AuthorId == userId || n.IsShared))
            .OrderByDescending(n => n.UpdatedAt)
            .Select(n => MapToResponse(n, userId))
            .ToListAsync();
    }

    public async Task<NpcNoteResponse> CreateNpcNoteAsync(long campaignId, CreateNpcNoteRequest request, long userId)
    {
        var isMember = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId);
        if (!isMember) throw new KeyNotFoundException("Campaign not found.");

        var note = new NpcNoteEntity
        {
            CampaignId = campaignId,
            AuthorId = userId,
            NpcName = request.NpcName.Trim(),
            Notes = request.Notes,
            IsShared = request.IsShared,
        };
        db.NpcNotes.Add(note);
        await db.SaveChangesAsync();
        await db.Entry(note).Reference(n => n.Author).LoadAsync();
        return MapToResponse(note, userId);
    }

    public async Task<NpcNoteResponse> UpdateNpcNoteAsync(long noteId, long campaignId, UpdateNpcNoteRequest request, long userId)
    {
        var note = await db.NpcNotes
            .Include(n => n.Author)
            .FirstOrDefaultAsync(n => n.Id == noteId && n.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Note not found.");

        if (note.AuthorId != userId) throw new UnauthorizedAccessException("You can only edit your own notes.");

        note.NpcName = request.NpcName.Trim();
        note.Notes = request.Notes;
        note.IsShared = request.IsShared;
        note.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return MapToResponse(note, userId);
    }

    public async Task DeleteNpcNoteAsync(long noteId, long campaignId, long userId)
    {
        var note = await db.NpcNotes
            .FirstOrDefaultAsync(n => n.Id == noteId && n.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Note not found.");

        var isGm = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId && m.Role == "gm");
        if (note.AuthorId != userId && !isGm) throw new UnauthorizedAccessException("You can only delete your own notes.");

        db.NpcNotes.Remove(note);
        await db.SaveChangesAsync();
    }

    private static NpcNoteResponse MapToResponse(NpcNoteEntity n, long userId) => new()
    {
        Id = n.Id,
        NpcName = n.NpcName,
        Notes = n.Notes,
        IsShared = n.IsShared,
        IsOwn = n.AuthorId == userId,
        AuthorName = n.Author?.DisplayName ?? string.Empty,
        CreatedAt = n.CreatedAt,
        UpdatedAt = n.UpdatedAt,
    };
}
