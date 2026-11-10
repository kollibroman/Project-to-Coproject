namespace ProjectCowork.Core.Features.Integrations.Github.Models;

public record GithubConnectionStatusDto
{
    public required bool IsConnected { get; init; }
    public string? GithubUsername { get; init; }
}
