namespace Messages.Notes.In;

public class CreateNoteRequest
{
    public long ToUserId { get; set; }
    public required string Content { get; set; }
}
