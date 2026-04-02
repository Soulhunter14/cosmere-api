namespace Messages.Campaigns.Out;

public class CampaignResponse
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? NextSessionDate { get; set; }
    public string? NextSessionTitle { get; set; }
}

public class CampaignDetailResponse
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Role { get; set; }
    public string? InviteCode { get; set; }
    public bool InviteActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<MemberResponse> Members { get; set; } = [];
}

public class MemberResponse
{
    public long UserId { get; set; }
    public required string DisplayName { get; set; }
    public required string Role { get; set; }
}
