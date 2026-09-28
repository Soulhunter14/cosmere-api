using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.DiceRolls.In;
using Messages.DiceRolls.Out;
using Microsoft.EntityFrameworkCore;

namespace Services.DiceRolls;

public class DiceRollService(CosmereContext db) : IDiceRollService
{
    public async Task<List<DiceRollResponse>> GetRecentAsync(long campaignId, long userId, int limit)
    {
        var isMember = await db.CampaignMembers
            .AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId);
        if (!isMember) throw new KeyNotFoundException("Campaign not found.");

        var rolls = await db.DiceRolls
            .Where(r => r.CampaignId == campaignId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(Math.Clamp(limit, 1, 100))
            .ToListAsync();

        // Return chronological order (oldest first) for chat-like display
        rolls.Reverse();
        return rolls.Select(MapToResponse).ToList();
    }

    public async Task<DiceRollResponse> CreateAsync(long campaignId, CreateDiceRollRequest request, long userId)
    {
        var member = await db.CampaignMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.CampaignId == campaignId && m.UserId == userId)
            ?? throw new KeyNotFoundException("Campaign not found.");

        var roll = new DiceRollEntity
        {
            CampaignId = campaignId,
            UserId = userId,
            UserDisplayName = member.User?.DisplayName ?? "Desconocido",
            RollType = request.RollType,
            RollData = request.RollData,
            RollLabel = request.RollLabel,
            CharacterName = string.IsNullOrWhiteSpace(request.CharacterName) ? null : request.CharacterName,
        };

        db.DiceRolls.Add(roll);
        await db.SaveChangesAsync();

        return MapToResponse(roll);
    }

    private static DiceRollResponse MapToResponse(DiceRollEntity r) => new()
    {
        Id = r.Id,
        CampaignId = r.CampaignId,
        UserId = r.UserId,
        UserDisplayName = r.UserDisplayName,
        RollType = r.RollType,
        RollData = r.RollData,
        RollLabel = r.RollLabel,
        CharacterName = r.CharacterName,
        CreatedAt = r.CreatedAt,
    };
}
