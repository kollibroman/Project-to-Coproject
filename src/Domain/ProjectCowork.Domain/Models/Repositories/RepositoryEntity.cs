using ProjectCowork.Domain.Models.Project;

namespace ProjectCowork.Domain.Models.Repositories;

public class RepositoryEntity : BaseTrackedEntity
{
    public Guid Id { get; init;  }
    
    public required string RepositoryName { get; init; }
    public required string RepoPath { get; set; }
    
    public required Guid ProjectId { get; set; }
    public ProjectEntity Project { get; set; } = null!;
}