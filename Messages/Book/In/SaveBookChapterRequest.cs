namespace Messages.Book.In;

public class SaveBookChapterRequest
{
    /// <summary>Chapter title («Ecos de lo perdido»).</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>The whole chapter as Markdown in the screen's scene format; it replaces the stored one.</summary>
    public string Md { get; set; } = string.Empty;
}
