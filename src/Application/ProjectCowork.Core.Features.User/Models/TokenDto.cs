namespace ProjectCowork.Core.Features.User.Models;

public record TokenDto
{
    public required string AccessToken { get; init; }
}