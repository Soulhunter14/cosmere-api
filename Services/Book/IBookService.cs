using Messages.Book.In;
using Messages.Book.Out;

namespace Services.Book;

/// <summary>The adventure book uploaded by the GM of a campaign, chapter by chapter («Libro» of the director's screen). GM only.</summary>
public interface IBookService
{
    /// <summary>The uploaded chapters in order, without their text (empty when nothing has been uploaded).</summary>
    Task<List<BookChapterSummaryResponse>> ListAsync(long campaignId, long userId);
    /// <summary>One chapter with its text; 404 when it has not been uploaded.</summary>
    Task<BookChapterResponse> GetAsync(long campaignId, int number, long userId);
    /// <summary>Creates the chapter or replaces the stored one.</summary>
    Task<BookChapterResponse> SaveAsync(long campaignId, int number, SaveBookChapterRequest request, long userId);
    /// <summary>Removes the chapter; nothing happens when it was not uploaded.</summary>
    Task DeleteAsync(long campaignId, int number, long userId);
}
