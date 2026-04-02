using Messages.Matches.In;
using Messages.Matches.Out;

namespace Services.Matches;

public interface IMatchService
{
    Task<List<MatchResponse>> GetMatchesAsync(long campaignId, long userId);
    Task<MatchResponse> GetMatchAsync(long matchId, long campaignId, long userId);
    Task<MatchResponse> CreateMatchAsync(long campaignId, CreateMatchRequest request, long userId);
    Task<MatchResponse> AddSceneAsync(long matchId, long campaignId, AddSceneRequest request, long userId);
    Task<MatchResponse> FinalizeMatchAsync(long matchId, long campaignId, FinalizeMatchRequest request, long userId);
    Task DeleteMatchAsync(long matchId, long campaignId, long userId);
}
