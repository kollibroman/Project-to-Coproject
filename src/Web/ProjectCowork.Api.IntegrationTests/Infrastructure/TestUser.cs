namespace ProjectCowork.Api.IntegrationTests.Infrastructure;

public sealed record TestUser
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required HttpClient Client { get; init; }
}
