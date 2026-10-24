namespace ProjectCowork.Core.Features.Repositories.Models;

public record RepositoryDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }

    public required string RepoUrl { get; init; }
    // TODO: files from packfile
}