namespace Messages.Notes.In;

public class CreateNoteRequest
{
    public List<long> ToUserIds { get; set; } = [];
    public required string Content { get; set; }
}
