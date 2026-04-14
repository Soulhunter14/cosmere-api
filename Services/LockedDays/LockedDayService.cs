using Infrastructure.Data;
using Messages.Database.Entities;
using Messages.LockedDays.In;
using Messages.LockedDays.Out;
using Microsoft.EntityFrameworkCore;

namespace Services.LockedDays;

public class LockedDayService(CosmereContext db) : ILockedDayService
{
    public async Task<List<LockedDayResponse>> GetLockedDaysAsync(long campaignId, long userId)
    {
        await EnsureMemberAsync(campaignId, userId);
        var isGm = await IsGmAsync(campaignId, userId);

        var query = db.LockedDays
            .Include(l => l.User)
            .Where(l => l.CampaignId == campaignId);

        if (!isGm)
            query = query.Where(l => l.UserId == userId);

        return await query
            .OrderBy(l => l.Date)
            .Select(l => MapToResponse(l))
            .ToListAsync();
    }

    public async Task<LockedDayResponse> AddLockedDayAsync(long campaignId, CreateLockedDayRequest request, long userId)
    {
        await EnsureMemberAsync(campaignId, userId);

        if (!DateOnly.TryParse(request.Date, out var date))
            throw new ArgumentException("Invalid date format. Use yyyy-MM-dd.");

        var alreadyLocked = await db.LockedDays
            .AnyAsync(l => l.CampaignId == campaignId && l.UserId == userId && l.Date == date);
        if (alreadyLocked)
            throw new InvalidOperationException("This day is already locked.");

        var user = await db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var entity = new LockedDayEntity
        {
            CampaignId = campaignId,
            UserId = userId,
            Date = date,
            Note = request.Note ?? string.Empty,
            User = user,
        };

        db.LockedDays.Add(entity);
        await db.SaveChangesAsync();
        return MapToResponse(entity);
    }

    public async Task RemoveLockedDayAsync(long lockedDayId, long campaignId, long userId)
    {
        await EnsureMemberAsync(campaignId, userId);
        var isGm = await IsGmAsync(campaignId, userId);

        var entity = await db.LockedDays
            .FirstOrDefaultAsync(l => l.Id == lockedDayId && l.CampaignId == campaignId)
            ?? throw new KeyNotFoundException("Locked day not found.");

        if (!isGm && entity.UserId != userId)
            throw new UnauthorizedAccessException("You can only remove your own locked days.");

        db.LockedDays.Remove(entity);
        await db.SaveChangesAsync();
    }

    private async Task<bool> IsGmAsync(long campaignId, long userId)
        => await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId && m.Role == "gm");

    private async Task EnsureMemberAsync(long campaignId, long userId)
    {
        var isMember = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId);
        if (!isMember) throw new KeyNotFoundException("Campaign not found.");
    }

    private static LockedDayResponse MapToResponse(LockedDayEntity l) => new()
    {
        Id = l.Id,
        CampaignId = l.CampaignId,
        UserId = l.UserId,
        UserDisplayName = l.User?.DisplayName ?? string.Empty,
        Date = l.Date.ToString("yyyy-MM-dd"),
        Note = l.Note,
        CreatedAt = l.CreatedAt,
    };
}
