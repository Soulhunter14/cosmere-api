namespace Messages.Auth.In;

public class RegisterRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string DisplayName { get; set; }
}
