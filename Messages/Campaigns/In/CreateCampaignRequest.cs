namespace Messages.Campaigns.In;

public class CreateCampaignRequest
{
    public required string Name { get; set; }
}

public class JoinCampaignRequest
{
    public required string InviteCode { get; set; }
}

public class UpdateInviteRequest
{
    public bool InviteActive { get; set; }
}
