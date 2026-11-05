using ProjectCowork.Infrastructure.Integrations.Git.Abstractions.Models;

namespace ProjectCowork.Infrastructure.Integrations.Git.Abstractions;

public interface IGitBrowserService
{
    IReadOnlyList<GitBranch>? GetBranches(Guid projectId, string repositoryName);

    IReadOnlyList<GitTreeItem>? GetTree(Guid projectId, string repositoryName, string branch, string path);

    GitFile? GetFile(Guid projectId, string repositoryName, string branch, string path);

    GitCommitsPage? GetCommits(Guid projectId, string repositoryName, string branch, int page, int pageSize,
        CancellationToken ct = default);
}
