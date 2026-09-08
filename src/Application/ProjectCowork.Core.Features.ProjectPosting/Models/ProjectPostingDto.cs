namespace ProjectCowork.Core.Features.ProjectPosting.Models;

public class ProjectPostingDto
{
    public required Guid Id { get; init; }
    public required string JobDescription { get; init; }
    public required string ProjectDescription { get; init; }
    public required Guid ProjectId { get; init; }
}