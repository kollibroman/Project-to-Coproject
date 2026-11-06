namespace ProjectCowork.Core.Features.Integrations.Github.Models;

public record UserRepositoryDto
{
    public required string Name { get; init; }
    public required string CloneUrl { get; init; }
    public required string OwnerName { get; init; }
}