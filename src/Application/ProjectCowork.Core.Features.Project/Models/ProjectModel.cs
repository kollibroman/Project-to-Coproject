namespace ProjectCowork.Core.Features.Models;

public record ProjectModel
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required Guid OwnerId { get; init; }
}