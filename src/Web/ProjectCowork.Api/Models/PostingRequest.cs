namespace ProjectCowork.Api.Models;

public record AddPostingRequest
{
    public required Guid ProjectId { get; init; }
    public required string JobDescription { get; init; }
    public required string ProjectDescription { get; init; }    
}

public record UpdatePostingRequest
{
    public required Guid ProjectPostingId { get; init; }
    public required string JobDescription { get; init; }
    public required string ProjectDescription { get; init; }
    public required bool IsActive { get; init; }
}

public record ApplyForProjectRequest
{
    public required Guid ProjectPostingId { get; init; }
    public required string Description { get; init; }
    public required IEnumerable<IFormFile> Attachments { get; init; }
}