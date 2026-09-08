namespace ProjectCowork.Core.Features.ProjectPosting.Models;

public class ProjectApplicationDto
{
    public required Guid Id { get; init; }
    public required string Description { get; init; }
    public required Guid UserId { get; init; }
}