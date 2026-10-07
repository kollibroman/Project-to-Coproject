namespace ProjectCowork.Infrastructure.Integrations.Git.Abstractions;

public interface IGitClientService
{
    Task<string> InitBareRepositoryAsync(string repositoryName, Guid repositoryId);
}