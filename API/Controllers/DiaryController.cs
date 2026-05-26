using Cross.Security;
using Messages.Diary.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Diary;

namespace API.Controllers;

[ApiController]
[Route("campaigns/{campaignId:long}/diary")]
[Authorize]
public class DiaryController(IDiaryService diaryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DiaryEntryResponse>>> GetEntries(long campaignId)
        => Ok(await diaryService.GetEntriesAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpGet("{entryId:long}")]
    public async Task<ActionResult<DiaryEntryResponse>> GetEntry(long campaignId, long entryId)
        => Ok(await diaryService.GetEntryAsync(entryId, campaignId, JwtHelper.GetUserId(User)));
}
