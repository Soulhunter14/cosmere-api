namespace Messages.Auth.Out;

public class LoginResponse
{
    public required string Token { get; set; }
    public required UserResponse User { get; set; }
}

public class UserResponse
{
    public long Id { get; set; }
    public required string Username { get; set; }
    public required string DisplayName { get; set; }
}
