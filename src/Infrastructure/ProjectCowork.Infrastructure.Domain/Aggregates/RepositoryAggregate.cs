using ProjectCowork.Domain.Models.Repositories;
using ProjectCowork.Infrastructure.Domain.Abstractions;
using ProjectCowork.Infrastructure.DomainEvents.Repositories;

namespace ProjectCowork.Infrastructure.Domain.Aggregates;

public class RepositoryAggregate : AggregateRoot
{
    private RepositoryEntity Repository { get; }
    
    public RepositoryAggregate(RepositoryEntity repositoryEntity)
    {
        ArgumentNullException.ThrowIfNull(repositoryEntity);
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
    
    public void DeleteRepository()
    {
        RaiseDomainEvent(new RepositoryDeletedEvent
        {
            RepositoryId = Repository.Id,
            ProjectId = Repository.ProjectId,
            RepositoryName = Repository.RepositoryName
        });
    }
}
