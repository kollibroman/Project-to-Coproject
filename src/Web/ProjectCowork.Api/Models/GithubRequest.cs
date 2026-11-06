namespace ProjectCowork.Api.Models;

public record GithubAuthorizePatRequest
{
    public required string Login { get; init; }
    public required string Pat { get; init; }
}

public record ImportGithubRepositoryRequest
{
    public required string RepositoryName { get; init; }
    public required Guid ProjectId { get; init; }
    public required string CloneUrl { get; init; }
}