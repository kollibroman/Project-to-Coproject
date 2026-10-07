using ProjectCowork.Domain.Models.Repositories;

namespace ProjectCowork.Infrastructure.Domain.Aggregates;

public class RepositoryAggregate
{
    private RepositoryEntity Repository { get; }
    
    public RepositoryAggregate(RepositoryEntity repositoryEntity)
    {
        Repository = repositoryEntity;
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
}