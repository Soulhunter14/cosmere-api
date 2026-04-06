namespace Messages.Notes.Out;

public class NoteResponse
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    public long FromUserId { get; set; }
    public long ToUserId { get; set; }
    public string FromDisplayName { get; set; } = string.Empty;
    public string ToDisplayName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
