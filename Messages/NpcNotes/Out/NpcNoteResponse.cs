namespace Messages.NpcNotes.Out;

public class NpcNoteResponse
{
    public long Id { get; set; }
    public string NpcName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsShared { get; set; }
    public bool IsOwn { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
