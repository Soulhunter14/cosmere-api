using Infrastructure.Data;
using Messages.Campaigns.In;
using Messages.Campaigns.Out;
using Messages.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Services.Worlds;

namespace Services.Campaigns;

public class CampaignService(CosmereContext db, IWorldRulesProvider reglas) : ICampaignService
{
    public async Task<List<CampaignResponse>> GetUserCampaignsAsync(long userId)
    {
        var now = DateTime.UtcNow;
        return await db.CampaignMembers
            .Where(m => m.UserId == userId)
            .Include(m => m.Campaign)
            .Select(m => new CampaignResponse
            {
                Id = m.Campaign.Id,
                Name = m.Campaign.Name,
                Role = m.Role,
                CreatedAt = m.Campaign.CreatedAt,
                World = m.Campaign.World,
                Era = m.Campaign.Era,
                NextSessionDate = m.Campaign.Sessions
                    .Where(s => s.Date > now)
                    .OrderBy(s => s.Date)
                    .Select(s => (DateTime?)s.Date)
                    .FirstOrDefault(),
                NextSessionTitle = m.Campaign.Sessions
                    .Where(s => s.Date > now)
                    .OrderBy(s => s.Date)
                    .Select(s => s.Title)
                    .FirstOrDefault(),
            })
            .ToListAsync();
    }

    public async Task<CampaignDetailResponse> GetCampaignDetailAsync(long campaignId, long userId)
    {
        var member = await db.CampaignMembers
            .FirstOrDefaultAsync(m => m.CampaignId == campaignId && m.UserId == userId)
            ?? throw new KeyNotFoundException("Campaign not found.");

        var campaign = await db.Campaigns
            .Include(c => c.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(c => c.Id == campaignId)
            ?? throw new KeyNotFoundException("Campaign not found.");

        return new CampaignDetailResponse
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Role = member.Role,
            InviteCode = member.Role == "gm" ? campaign.InviteCode : null,
            InviteActive = campaign.InviteActive,
            CreatedAt = campaign.CreatedAt,
            World = campaign.World,
            Era = campaign.Era,
            Members = campaign.Members.Select(m => new MemberResponse
            {
                UserId = m.UserId,
                DisplayName = m.User.DisplayName,
                Role = m.Role
            }).ToList()
        };
    }

    public async Task<CampaignDetailResponse> CreateCampaignAsync(CreateCampaignRequest request, long userId)
    {
        // The registered IWorldRules decide which worlds exist and how each one validates its era.
        if (!reglas.Existe(request.World))
            throw new ArgumentException($"Invalid World: '{request.World}'.");
        var era = reglas.Get(request.World).NormalizarEra(request.Era);

        var campaign = new CampaignEntity
        {
            Name = request.Name,
            GmUserId = userId,
            InviteCode = GenerateInviteCode(),
            World = request.World,
            Era = era
        };

        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync();

        db.CampaignMembers.Add(new CampaignMemberEntity
        {
            CampaignId = campaign.Id,
            UserId = userId,
            Role = "gm"
        });
        await db.SaveChangesAsync();

        return await GetCampaignDetailAsync(campaign.Id, userId);
    }

    public async Task DeleteCampaignAsync(long campaignId, long userId)
    {
        var campaign = await db.Campaigns.FirstOrDefaultAsync(c => c.Id == campaignId && c.GmUserId == userId)
            ?? throw new KeyNotFoundException("Campaign not found.");

        db.Campaigns.Remove(campaign);
        await db.SaveChangesAsync();
    }

    public async Task<CampaignResponse> JoinCampaignAsync(JoinCampaignRequest request, long userId)
    {
        var campaign = await db.Campaigns.FirstOrDefaultAsync(c => c.InviteCode == request.InviteCode && c.InviteActive)
            ?? throw new KeyNotFoundException("Invalid or inactive invite code.");

        var alreadyMember = await db.CampaignMembers.AnyAsync(m => m.CampaignId == campaign.Id && m.UserId == userId);
        if (alreadyMember)
            throw new InvalidOperationException("Already a member of this campaign.");

        db.CampaignMembers.Add(new CampaignMemberEntity
        {
            CampaignId = campaign.Id,
            UserId = userId,
            Role = "player"
        });
        await db.SaveChangesAsync();

        var member = await db.CampaignMembers.FirstAsync(m => m.CampaignId == campaign.Id && m.UserId == userId);
        return new CampaignResponse
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Role = member.Role,
            CreatedAt = campaign.CreatedAt,
            World = campaign.World,
            Era = campaign.Era
        };
    }

    public async Task UpdateInviteAsync(long campaignId, UpdateInviteRequest request, long userId)
    {
        var campaign = await GetGmCampaignAsync(campaignId, userId);
        campaign.InviteActive = request.InviteActive;
        await db.SaveChangesAsync();
    }

    public async Task<string> RegenerateInviteCodeAsync(long campaignId, long userId)
    {
        var campaign = await GetGmCampaignAsync(campaignId, userId);
        campaign.InviteCode = GenerateInviteCode();
        await db.SaveChangesAsync();
        return campaign.InviteCode;
    }

    private async Task<CampaignEntity> GetGmCampaignAsync(long campaignId, long userId)
    {
        var campaign = await db.Campaigns.FirstOrDefaultAsync(c => c.Id == campaignId && c.GmUserId == userId)
            ?? throw new KeyNotFoundException("Campaign not found.");
        return campaign;
    }

    private static string GenerateInviteCode() =>
        Guid.NewGuid().ToString("N")[..6].ToUpper();
}
