namespace ProjectCowork.Api.Models;

public record AccessTokenResponse
{
    public required string AccessToken { get; init; }
}
