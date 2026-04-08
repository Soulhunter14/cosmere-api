namespace Messages.NpcNotes.In;

public class CreateNpcNoteRequest
{
    public string NpcName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsShared { get; set; } = false;
}

public class UpdateNpcNoteRequest
{
    public string NpcName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsShared { get; set; } = false;
}
