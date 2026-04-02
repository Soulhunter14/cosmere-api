namespace Messages.SideQuests.In;

public class CreateSideQuestRequest
{
    public required string Name { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Acts { get; set; } = [];
    public List<string> Rewards { get; set; } = [];
    public List<string> Benefits { get; set; } = [];
    public string Notes { get; set; } = string.Empty;
}

public class UpdateSideQuestRequest : CreateSideQuestRequest
{
    public bool Started { get; set; }
}
