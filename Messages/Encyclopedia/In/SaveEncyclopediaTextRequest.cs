namespace Messages.Encyclopedia.In;

public class SaveEncyclopediaTextRequest
{
    /// <summary>Title of the entry («Agente», «Alomancia de acero»).</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>The book's text as Markdown; it replaces the stored one.</summary>
    public string Md { get; set; } = string.Empty;
}
