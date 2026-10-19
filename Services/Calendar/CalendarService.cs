using Infrastructure.Data;
using Messages.Calendar.Out;
using Microsoft.EntityFrameworkCore;

namespace Services.Calendar;

public class CalendarService(CosmereContext db) : ICalendarService
{
    public async Task<List<OtherCampaignEventResponse>> GetOtherCampaignsAsync(long campaignId, long userId)
    {
        var isMember = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaignId && m.UserId == userId);
        if (!isMember) throw new KeyNotFoundException("Campaign not found.");

        var others = await db.CampaignMembers
            .Where(m => m.UserId == userId && m.CampaignId != campaignId)
            .Select(m => new { m.CampaignId, m.Campaign.Name, m.Campaign.World, m.Role })
            .ToListAsync();
        if (others.Count == 0) return [];

        var byId = others.ToDictionary(o => o.CampaignId);
        var ids = byId.Keys.ToList();

        var sessions = await db.Sessions
            .Where(s => ids.Contains(s.CampaignId))
            .Select(s => new { s.CampaignId, s.Id, s.Title, s.Date, s.Location })
            .ToListAsync();

        // Pending slots of open proposals that are still ahead (from today, UTC)
        var today = DateTime.UtcNow.Date;
        var slots = await db.ProposalDates
            .Where(d => ids.Contains(d.Proposal.CampaignId)
                        && d.Proposal.Status == "Pending"
                        && d.Status == "Pending"
                        && d.ProposedDate >= today)
            .Select(d => new { d.Proposal.CampaignId, d.Id, d.Proposal.Title, d.ProposedDate, d.Slot })
            .ToListAsync();

        var events = sessions.Select(s => new OtherCampaignEventResponse
        {
            CampaignId = s.CampaignId,
            CampaignName = byId[s.CampaignId].Name,
            World = byId[s.CampaignId].World,
            Role = byId[s.CampaignId].Role,
            Kind = "session",
            Id = s.Id,
            Title = s.Title,
            Date = s.Date,
            Location = s.Location,
        }).Concat(slots.Select(d => new OtherCampaignEventResponse
        {
            CampaignId = d.CampaignId,
            CampaignName = byId[d.CampaignId].Name,
            World = byId[d.CampaignId].World,
            Role = byId[d.CampaignId].Role,
            Kind = "proposal",
            Id = d.Id,
            Title = d.Title,
            Date = d.ProposedDate,
            Slot = d.Slot,
        }));

        return events.OrderBy(e => e.Date).ToList();
    }
}
