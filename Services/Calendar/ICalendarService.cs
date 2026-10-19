using Messages.Calendar.Out;

namespace Services.Calendar;

public interface ICalendarService
{
    /// <summary>Sessions and pending proposed slots of every other campaign the user belongs to (any role).</summary>
    Task<List<OtherCampaignEventResponse>> GetOtherCampaignsAsync(long campaignId, long userId);
}
