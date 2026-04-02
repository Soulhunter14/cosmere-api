using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.SideQuests.In;
using Messages.SideQuests.Out;
using Microsoft.EntityFrameworkCore;

namespace Services.SideQuests;

public class SideQuestService(CosmereContext db) : ISideQuestService
{
    public async Task<List<SideQuestResponse>> GetSideQuestsAsync(long campaignId, long userId)
    {
        var isGm = await IsGmAsync(campaignId, userId);
        if (!isGm)
        {
            var isMember = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId);
            if (!isMember) throw new KeyNotFoundException("Campaign not found.");
        }

        var query = db.SideQuests.Where(sq => sq.CampaignId == campaignId);
        if (!isGm) query = query.Where(sq => sq.Started);

        var quests = await query.OrderBy(sq => sq.CreatedAt).ToListAsync();
        return quests.Select(sq => MapToResponse(sq, isGm)).ToList();
    }

    public async Task<SideQuestResponse> GetSideQuestAsync(long sideQuestId, long campaignId, long userId)
    {
        var isGm = await IsGmAsync(campaignId, userId);
        var quest = await db.SideQuests
            .FirstOrDefaultAsync(sq => sq.Id == sideQuestId && sq.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Side quest not found.");
        if (!isGm && !quest.Started) throw new KeyNotFoundException("Side quest not found.");
        return MapToResponse(quest, isGm);
    }

    public async Task<SideQuestResponse> CreateSideQuestAsync(long campaignId, CreateSideQuestRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);
        var quest = new SideQuestEntity
        {
            CampaignId = campaignId,
            Name = request.Name,
            Summary = request.Summary,
            Description = request.Description,
            Acts = request.Acts,
            Rewards = request.Rewards,
            Benefits = request.Benefits,
            Notes = request.Notes
        };
        db.SideQuests.Add(quest);
        await db.SaveChangesAsync();
        return MapToResponse(quest, true);
    }

    public async Task<SideQuestResponse> UpdateSideQuestAsync(long sideQuestId, long campaignId, UpdateSideQuestRequest request, long userId)
    {
        await EnsureGmAsync(campaignId, userId);
        var quest = await db.SideQuests
            .FirstOrDefaultAsync(sq => sq.Id == sideQuestId && sq.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Side quest not found.");

        quest.Name = request.Name; quest.Summary = request.Summary;
        quest.Description = request.Description; quest.Acts = request.Acts;
        quest.Rewards = request.Rewards; quest.Benefits = request.Benefits;
        quest.Notes = request.Notes; quest.Started = request.Started;
        await db.SaveChangesAsync();
        return MapToResponse(quest, true);
    }

    public async Task DeleteSideQuestAsync(long sideQuestId, long campaignId, long userId)
    {
        await EnsureGmAsync(campaignId, userId);
        var quest = await db.SideQuests
            .FirstOrDefaultAsync(sq => sq.Id == sideQuestId && sq.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Side quest not found.");
        db.SideQuests.Remove(quest);
        await db.SaveChangesAsync();
    }

    private async Task<bool> IsGmAsync(long campaignId, long userId) =>
        await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId && m.Role == "gm");

    private async Task EnsureGmAsync(long campaignId, long userId)
    {
        if (!await IsGmAsync(campaignId, userId))
            throw new UnauthorizedAccessException("Only the GM can perform this action.");
    }

    private static SideQuestResponse MapToResponse(SideQuestEntity sq, bool includeNotes) => new()
    {
        Id = sq.Id, CampaignId = sq.CampaignId, Name = sq.Name, Summary = sq.Summary,
        Description = sq.Description, Acts = sq.Acts, Rewards = sq.Rewards,
        Benefits = sq.Benefits, Notes = includeNotes ? sq.Notes : string.Empty,
        Started = sq.Started, CreatedAt = sq.CreatedAt
    };
}
