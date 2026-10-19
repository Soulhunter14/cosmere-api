namespace Messages.Encyclopedia.Out;

/// <summary>An uploaded original in the list of the world: what it has, without the text or the PDF.</summary>
public class EncyclopediaOriginalSummaryResponse
{
    public string Section { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    /// <summary>Length of the Markdown text in characters; null = no text uploaded.</summary>
    public int? TextLength { get; set; }
    /// <summary>Size of the mini-PDF in bytes; null = no PDF uploaded.</summary>
    public int? PdfBytes { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>The original text of an entry.</summary>
public class EncyclopediaTextResponse
{
    public string Section { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Md { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
