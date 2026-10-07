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

    public Task<string> InitBareRepositoryAsync(string repositoryName, Guid repositoryId)
    {
        var baseRepoFolder = Path.Combine(Directory.GetCurrentDirectory(), "Repos", repositoryId.ToString());
        var gitPath = Path.Combine(baseRepoFolder, $"{repositoryName}.git");

        Repository.Init(gitPath, isBare: true);

        using var repo = new Repository(gitPath);
        repo.Config.Set("http.receivepack", true);
        
        var request =  _httpContextAccessor.HttpContext.Request;
        
        var baseUrl = $"{request.Scheme}://{request.Host}";
        var cloneUrl = $"{baseUrl}/api/git/{repositoryId}/{repositoryName}.git";
        
        return Task.FromResult(cloneUrl);
    }
}