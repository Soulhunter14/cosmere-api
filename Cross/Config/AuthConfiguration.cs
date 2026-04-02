namespace Cross.Config;

public class AuthConfiguration
{
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string SecretKey { get; set; }
    public int ExpirationHours { get; set; } = 24;
}
