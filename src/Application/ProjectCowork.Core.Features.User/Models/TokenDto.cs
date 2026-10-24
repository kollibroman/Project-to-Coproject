namespace ProjectCowork.Core.Features.User.Models;

public record TokenDto
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime RefreshTokenExpiresAt { get; init; }
}
