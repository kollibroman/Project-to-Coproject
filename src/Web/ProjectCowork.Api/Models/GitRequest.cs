namespace ProjectCowork.Api.Models;

public record CreateRepositoryRequest
{
    public required string Name { get; init; }   
    public required Guid ProjectId { get; init; }   
}