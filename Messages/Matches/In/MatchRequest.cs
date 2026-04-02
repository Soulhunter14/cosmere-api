namespace Messages.Matches.In;

public class CreateMatchRequest
{
    public string Notes { get; set; } = string.Empty;
}

public class AddSceneRequest
{
    public required string Description { get; set; }
}

public class FinalizeMatchRequest
{
    public required string Resolution { get; set; }
}
