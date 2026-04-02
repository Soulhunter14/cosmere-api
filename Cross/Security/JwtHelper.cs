using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cross.Config;
using Microsoft.IdentityModel.Tokens;

namespace Cross.Security;

public static class JwtHelper
{
    public static string GenerateToken(long userId, string username, string displayName, AuthConfiguration config)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("username", username),
            new Claim("displayName", displayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config.Issuer,
            audience: config.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(config.ExpirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static long GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? throw new UnauthorizedAccessException("Invalid token.");
        return long.Parse(sub);
    }
}
