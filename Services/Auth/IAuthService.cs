using Messages.Auth.In;
using Messages.Auth.Out;

namespace Services.Auth;

public interface IAuthService
{
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
