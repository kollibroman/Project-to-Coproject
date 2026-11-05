using ProjectCowork.Infrastructure.Integrations.Github.Models;

namespace ProjectCowork.Infrastructure.Integrations.Github.Abstractions;

public interface IGithubService
{
    Task<IEnumerable<RepositoryModel>?> FetchUserRepositoriesAsync(string login, string pat, CancellationToken ct);
}