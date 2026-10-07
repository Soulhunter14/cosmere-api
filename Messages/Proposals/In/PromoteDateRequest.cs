namespace Messages.Proposals.In;

public class PromoteDateRequest
{
    public required string Title { get; set; }
    public string Location { get; set; } = string.Empty;
}
