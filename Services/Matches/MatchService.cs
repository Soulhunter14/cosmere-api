using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.Matches.In;
using Messages.Matches.Out;
using Microsoft.EntityFrameworkCore;

namespace Services.Matches;

public class MatchService(CosmereContext db) : IMatchService
{
    public async Task<List<MatchResponse>> GetMatchesAsync(long campaignId, long userId)
    {
        await EnsureMemberAsync(campaignId, userId);
        return await db.Matches
            .Where(m => m.CampaignId == campaignId)
            .Include(m => m.Scenes)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => MapToResponse(m))
            .ToListAsync();
    }

    public async Task<MatchResponse> GetMatchAsync(long matchId, long campaignId, long userId)
    {
        await EnsureMemberAsync(campaignId, userId);
        var match = await db.Matches
            .Include(m => m.Scenes)
            .FirstOrDefaultAsync(m => m.Id == matchId && m.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Match not found.");
        return MapToResponse(match);
    }

    public async Task<MatchResponse> CreateMatchAsync(long campaignId, CreateMatchRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);
        var match = new MatchEntity { CampaignId = campaignId };
        db.Matches.Add(match);
        await db.SaveChangesAsync();
        return MapToResponse(match);
    }

    public async Task<MatchResponse> AddSceneAsync(long matchId, long campaignId, AddSceneRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);
        var match = await db.Matches
            .Include(m => m.Scenes)
            .FirstOrDefaultAsync(m => m.Id == matchId && m.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Match not found.");

        if (match.IsCompleted) throw new InvalidOperationException("Cannot add scenes to a completed match.");

        var scene = new SceneEntity
        {
            MatchId = matchId,
            Description = request.Description,
            OrderIndex = match.Scenes.Count
        };
        db.Scenes.Add(scene);
        await db.SaveChangesAsync();

        match.Scenes.Add(scene);
        return MapToResponse(match);
    }

    public async Task<MatchResponse> FinalizeMatchAsync(long matchId, long campaignId, FinalizeMatchRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);
        var match = await db.Matches
            .Include(m => m.Scenes)
            .FirstOrDefaultAsync(m => m.Id == matchId && m.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Match not found.");

        match.Resolution = request.Resolution;
        match.IsCompleted = true;
        match.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return MapToResponse(match);
    }

    public async Task DeleteMatchAsync(long matchId, long campaignId, long userId)
    {
        await EnsureGmAsync(campaignId, userId);
        var match = await db.Matches.FirstOrDefaultAsync(m => m.Id == matchId && m.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Match not found.");
        db.Matches.Remove(match);
        await db.SaveChangesAsync();
    }

    private async Task EnsureMemberAsync(long campaignId, long userId)
    {
        var isMember = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId);
        if (!isMember) throw new KeyNotFoundException("Campaign not found.");
    }

    private async Task EnsureGmAsync(long campaignId, long userId)
    {
        var isGm = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId && m.Role == "gm");
        if (!isGm) throw new UnauthorizedAccessException("Only the GM can perform this action.");
    }

    private static MatchResponse MapToResponse(MatchEntity m) => new()
    {
        Id = m.Id, CampaignId = m.CampaignId, Resolution = m.Resolution,
        IsCompleted = m.IsCompleted, CreatedAt = m.CreatedAt, CompletedAt = m.CompletedAt,
        Scenes = m.Scenes.OrderBy(s => s.OrderIndex).Select(s => new SceneResponse
        {
            Id = s.Id, Description = s.Description, OrderIndex = s.OrderIndex
        }).ToList()
    };
}
