namespace Messages.Campaigns.Out;

/// <summary>A registered user who is not a member of the campaign yet, offered to the GM to add.</summary>
public class UserCandidateResponse
{
    public long UserId { get; set; }
    public required string Username { get; set; }
    public required string DisplayName { get; set; }
}
