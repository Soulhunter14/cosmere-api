namespace Messages.Book.Out;

/// <summary>A chapter of the uploaded book in the list: without its text, which is fetched one chapter at a time.</summary>
public class BookChapterSummaryResponse
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    /// <summary>Length of the Markdown text, in characters.</summary>
    public int Length { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>A chapter of the uploaded book with its Markdown text.</summary>
public class BookChapterResponse
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Md { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
