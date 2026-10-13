using LibGit2Sharp;
using Microsoft.AspNetCore.Http;
using ProjectCowork.Infrastructure.Integrations.Git.Abstractions;

namespace ProjectCowork.Infrastructure.Integrations.Git.Internal;

internal class GitClientService : IGitClientService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GitClientService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<string> InitBareRepositoryAsync(string repositoryName, Guid projectId)
    {
        // Repos/{projectId}/{repositoryName}.git - one folder per project, many repos inside.
        var projectRoot = GitRepositoryPaths.GetProjectRoot(projectId);
        var gitPath = GitRepositoryPaths.GetRepositoryPath(projectId, repositoryName);

        if (Directory.Exists(gitPath))
            throw new InvalidOperationException(
                $"Repository '{repositoryName}' already exists in project {projectId}.");

        Directory.CreateDirectory(projectRoot);
        Repository.Init(gitPath, isBare: true);

        using (var repo = new Repository(gitPath))
        {
            repo.Config.Set("http.receivepack", true);
        }

        File.WriteAllText(Path.Combine(gitPath, "HEAD"), "ref: refs/heads/main\n");

        var request = _httpContextAccessor.HttpContext!.Request;
        var cloneUrl = $"{request.Scheme}://{request.Host}/api/git/{projectId}/{repositoryName}.git";

        return Task.FromResult(cloneUrl);
    }
}
