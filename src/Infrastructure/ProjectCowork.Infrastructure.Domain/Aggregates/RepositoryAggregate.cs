using ProjectCowork.Domain.Models.Project;
using ProjectCowork.Domain.Models.Repositories;

namespace ProjectCowork.Infrastructure.Domain.Aggregates;

public class RepositoryAggregate
{
    private RepositoryEntity Repository { get; }
    private ProjectEntity Project { get; }
    
    public RepositoryAggregate(RepositoryEntity repositoryEntity, ProjectEntity project)
    {
        Repository = repositoryEntity;
        Project = project;
    }

    public static RepositoryEntity Create(string repositoryName, string repoPath, Guid projectId)
    {
        return new RepositoryEntity
        {
            Id = Guid.NewGuid(),
            RepositoryName = repositoryName,
            RepoPath = repoPath,
            ProjectId = projectId
        };
    }
    
    public void DeleteRepositoryAsync()
    {
        Project.Repositories.Remove(Repository);

        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Repos", $"{Repository.ProjectId}");
        
        var directoryExists = Directory.Exists(folderPath);

        if (!directoryExists)
        {
            return;
        }
        
        Directory.Delete(folderPath, true);
    }
}