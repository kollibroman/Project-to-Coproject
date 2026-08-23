namespace ProjectCowork.Api.Models;

public record LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

public record RegisterRequest : LoginRequest
{
    public required string Username { get; init; }
}