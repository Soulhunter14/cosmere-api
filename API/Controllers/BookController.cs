using Cross.Security;
using Messages.Book.In;
using Messages.Book.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Book;

namespace API.Controllers;

/// <summary>The adventure book uploaded by the GM («Libro» of the director's screen with the book's original text). GM only.</summary>
[ApiController]
[Route("campaigns/{campaignId:long}/book")]
[Authorize]
public class BookController(IBookService bookService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BookChapterSummaryResponse>>> GetChapters(long campaignId)
        => Ok(await bookService.ListAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpGet("{number:int}")]
    public async Task<ActionResult<BookChapterResponse>> GetChapter(long campaignId, int number)
        => Ok(await bookService.GetAsync(campaignId, number, JwtHelper.GetUserId(User)));

    [HttpPut("{number:int}")]
    public async Task<ActionResult<BookChapterResponse>> SaveChapter(long campaignId, int number, [FromBody] SaveBookChapterRequest request)
        => Ok(await bookService.SaveAsync(campaignId, number, request, JwtHelper.GetUserId(User)));

    [HttpDelete("{number:int}")]
    public async Task<IActionResult> DeleteChapter(long campaignId, int number)
    {
        await bookService.DeleteAsync(campaignId, number, JwtHelper.GetUserId(User));
        return NoContent();
    }
}
