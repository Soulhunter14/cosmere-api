using Messages.LockedDays.In;
using Messages.LockedDays.Out;

namespace Services.LockedDays;

public interface ILockedDayService
{
    Task<List<LockedDayResponse>> GetLockedDaysAsync(long campaignId, long userId);
    Task<LockedDayResponse> AddLockedDayAsync(long campaignId, CreateLockedDayRequest request, long userId);
    Task RemoveLockedDayAsync(long lockedDayId, long campaignId, long userId);
}
