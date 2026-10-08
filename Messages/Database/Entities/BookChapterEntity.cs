namespace Messages.Database.Entities;

/// <summary>
/// A chapter of the adventure book uploaded by the GM of a campaign: the «Libro» of the director's screen with the book's original
/// text, as Markdown in the screen's scene format (`docs/pantalla-director/guion-formato.md`). The web client reads it; the server
/// never reads inside it. One row per campaign and chapter number (AddBookChapters).
/// </summary>
public class BookChapterEntity
{
    public long Id { get; set; }
    public long CampaignId { get; set; }
    /// <summary>Chapter number in the book (1-99); unique per campaign.</summary>
    public int Number { get; set; }
    public required string Title { get; set; }
    /// <summary>The chapter as Markdown text.</summary>
    public string Md { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public CampaignEntity Campaign { get; set; } = null!;
}
