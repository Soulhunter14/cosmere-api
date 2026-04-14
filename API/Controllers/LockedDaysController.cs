using Cross.Security;
using Messages.LockedDays.In;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.LockedDays;

namespace API.Controllers;

[ApiController]
[Route("campaigns/{campaignId:long}/locked-days")]
[Authorize]
public class LockedDaysController(ILockedDayService lockedDayService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(long campaignId)
        => Ok(await lockedDayService.GetLockedDaysAsync(campaignId, JwtHelper.GetUserId(User)));

    [HttpPost]
    public async Task<IActionResult> Add(long campaignId, [FromBody] CreateLockedDayRequest request)
        => Ok(await lockedDayService.AddLockedDayAsync(campaignId, request, JwtHelper.GetUserId(User)));

    [HttpDelete("{lockedDayId:long}")]
    public async Task<IActionResult> Remove(long campaignId, long lockedDayId)
    {
        await lockedDayService.RemoveLockedDayAsync(lockedDayId, campaignId, JwtHelper.GetUserId(User));
        return NoContent();
    }
}
