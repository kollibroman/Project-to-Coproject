namespace ProjectCowork.Api.Models;

public abstract record ProjectRequestBase
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}

public record CreateProjectRequest : ProjectRequestBase
{
}

public record EditProjectRequest : ProjectRequestBase
{
    public required Guid ProjectId { get; init; }
}