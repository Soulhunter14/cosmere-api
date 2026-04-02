using Cross.Config;
using Cross.Security;
using Infrastructure.Data;
using Messages.Auth.In;
using Messages.Auth.Out;
using Messages.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Services.Auth;

public class AuthService(CosmereContext db, IOptions<AuthConfiguration> authConfig) : IAuthService
{
    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        var exists = await db.Users.AnyAsync(u => u.Username == request.Username.ToLower());
        if (exists)
            throw new InvalidOperationException("Username already taken.");

        var user = new UserEntity
        {
            Username = request.Username.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = JwtHelper.GenerateToken(user.Id, user.Username, user.DisplayName, authConfig.Value);
        return BuildResponse(user, token);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username.ToLower())
            ?? throw new KeyNotFoundException("Invalid username or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new KeyNotFoundException("Invalid username or password.");

        var token = JwtHelper.GenerateToken(user.Id, user.Username, user.DisplayName, authConfig.Value);
        return BuildResponse(user, token);
    }

    private static LoginResponse BuildResponse(UserEntity user, string token) => new()
    {
        Token = token,
        User = new UserResponse { Id = user.Id, Username = user.Username, DisplayName = user.DisplayName }
    };
}
