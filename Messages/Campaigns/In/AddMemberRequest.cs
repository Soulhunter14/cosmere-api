namespace Messages.Campaigns.In;

/// <summary>An already registered user that the GM adds to the campaign as a player.</summary>
public class AddMemberRequest
{
    public long UserId { get; set; }
}
