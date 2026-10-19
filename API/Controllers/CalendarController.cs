using Cross.Security;
using Messages.Calendar.Out;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Calendar;

namespace API.Controllers;

[ApiController]
[Route("calendar")]
[Authorize]
public class CalendarController(ICalendarService calendarService) : ControllerBase
{
    /// <summary>What the user has in their other campaigns (read-only), seen from the calendar of <paramref name="campaignId"/>.</summary>
    [HttpGet("others")]
    public async Task<ActionResult<List<OtherCampaignEventResponse>>> GetOthers([FromQuery] long campaignId)
        => Ok(await calendarService.GetOtherCampaignsAsync(campaignId, JwtHelper.GetUserId(User)));
}
