using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.Sessions.In;
using Messages.Sessions.Out;
using Microsoft.EntityFrameworkCore;

namespace Services.Sessions;

public class SessionService(CosmereContext db) : ISessionService
{
    public async Task<List<SessionResponse>> GetSessionsAsync(long campaignId, long userId)
    {
        var isMember = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId);
        if (!isMember) throw new KeyNotFoundException("Campaign not found.");

        var sessions = await db.Sessions
            .Where(s => s.CampaignId == campaignId)
            .OrderBy(s => s.Date)
            .ToListAsync();

        return sessions.Select(MapToResponse).ToList();
    }

    public async Task<SessionResponse> CreateSessionAsync(long campaignId, CreateSessionRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        var session = new SessionEntity
        {
            CampaignId = campaignId,
            Title = request.Title,
            Date = request.Date.ToUniversalTime(),
            Location = request.Location,
            Notes = request.Notes,
        };

        db.Sessions.Add(session);
        await db.SaveChangesAsync();
        return MapToResponse(session);
    }

    public async Task DeleteSessionAsync(long sessionId, long campaignId, long userId)
    {
        await EnsureGmAsync(campaignId, userId);

        var session = await db.Sessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Session not found.");

        db.Sessions.Remove(session);
        await db.SaveChangesAsync();
    }

    private async Task<bool> IsGmAsync(long campaignId, long userId) =>
        await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId && m.Role == "gm");

    private async Task EnsureGmAsync(long campaignId, long userId)
    {
        if (!await IsGmAsync(campaignId, userId))
            throw new UnauthorizedAccessException("Only the GM can perform this action.");
    }

    private static SessionResponse MapToResponse(SessionEntity s) => new()
    {
        Id = s.Id,
        CampaignId = s.CampaignId,
        Title = s.Title,
        Date = s.Date,
        Location = s.Location,
        Notes = s.Notes,
        CreatedAt = s.CreatedAt,
    };
}
