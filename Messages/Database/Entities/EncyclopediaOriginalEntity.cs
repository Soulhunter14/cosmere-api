namespace Messages.Database.Entities;

/// <summary>
/// The original of an encyclopedia entry (AddEncyclopediaOriginals): the book's text of a heroic path, a metalborn path, a power
/// of the metallic arts… as Markdown, and a mini-PDF with only its pages, both generated from the GM's own PDF by
/// `scripts/enciclopedia/generar_originales.py` (web repository) and uploaded by a GM. Shared by every campaign of its world, like
/// the book images of the adversaries. One row per world, section and key; the text and the PDF are uploaded and removed apart
/// (null = not uploaded). The server never reads inside them.
/// </summary>
public class EncyclopediaOriginalEntity
{
    public long Id { get; set; }
    /// <summary>A world id (`WorldIds`): the world of the campaign it was uploaded through.</summary>
    public required string World { get; set; }
    /// <summary>Section of the encyclopedia («caminos-heroicos», «caminos-metal», «artes-metalicas»…).</summary>
    public required string Section { get; set; }
    /// <summary>Entry of the section («agente», «alomancia-acero»…).</summary>
    public required string Key { get; set; }
    public required string Title { get; set; }
    /// <summary>The book's text as Markdown; null = not uploaded.</summary>
    public string? Md { get; set; }
    /// <summary>The mini-PDF with the pages of the entry; null = not uploaded.</summary>
    public byte[]? Pdf { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
