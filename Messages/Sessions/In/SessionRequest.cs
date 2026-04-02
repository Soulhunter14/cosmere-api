namespace Messages.Sessions.In;

public class CreateSessionRequest
{
    public required string Title { get; set; }
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
