using Messages.Sessions.In;
using Messages.Sessions.Out;

namespace Services.Sessions;

public interface ISessionService
{
    Task<List<SessionResponse>> GetSessionsAsync(long campaignId, long userId);
    Task<SessionResponse> CreateSessionAsync(long campaignId, CreateSessionRequest request, long userId);
    Task DeleteSessionAsync(long sessionId, long campaignId, long userId);
}
