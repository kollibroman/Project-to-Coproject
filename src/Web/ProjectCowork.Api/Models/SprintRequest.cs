namespace ProjectCowork.Api.Models;

public abstract record SprintRequestBase
{
    public required string Name { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
}

public record AddSprintRequest : SprintRequestBase
{
    public required Guid ProjectId { get; init; }
}

public record EditSprintRequest : SprintRequestBase
{
}
