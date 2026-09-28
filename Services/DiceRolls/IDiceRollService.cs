using Messages.DiceRolls.In;
using Messages.DiceRolls.Out;

namespace Services.DiceRolls;

public interface IDiceRollService
{
    Task<List<DiceRollResponse>> GetRecentAsync(long campaignId, long userId, int limit);
    Task<DiceRollResponse> CreateAsync(long campaignId, CreateDiceRollRequest request, long userId);
}
