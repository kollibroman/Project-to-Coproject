namespace ProjectCowork.Infrastructure.Integrations.Git.Abstractions;

public interface IGitClientService
{
    Task<string> InitBareRepositoryAsync(string repositoryName, Guid projectId);
    Task<string> CloneGithubRepositoryAsync(string cloneUrl, string repositoryName, Guid projectId);
}