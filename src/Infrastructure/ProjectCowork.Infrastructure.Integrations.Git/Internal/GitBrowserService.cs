using LibGit2Sharp;
using ProjectCowork.Infrastructure.Integrations.Git.Abstractions;
using ProjectCowork.Infrastructure.Integrations.Git.Abstractions.Models;

namespace ProjectCowork.Infrastructure.Integrations.Git.Internal;

internal class GitBrowserService : IGitBrowserService
{
    private const long MaxPreviewBytes = 1024 * 1024;

    public IReadOnlyList<GitBranch>? GetBranches(Guid projectId, string repositoryName)
    {
        using var repo = Open(projectId, repositoryName);
        if (repo is null)
            return null;

        var headTarget = repo.Refs.Head?.TargetIdentifier;

        return repo.Branches
            .Where(b => !b.IsRemote && b.Tip is not null)
            .Select(b => new GitBranch(b.FriendlyName, b.CanonicalName == headTarget))
            .OrderByDescending(b => b.IsDefault)
            .ThenBy(b => b.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public IReadOnlyList<GitTreeItem>? GetTree(Guid projectId, string repositoryName, string branch, string path)
    {
        using var repo = Open(projectId, repositoryName);
        var commit = repo is null ? null : ResolveBranchTip(repo, branch);
        if (commit is null)
        {
            return null;
        }

        var normalizedPath = NormalizePath(path);
        var tree = normalizedPath.Length == 0
            ? commit.Tree
            : commit[normalizedPath]?.Target as Tree;

        if (tree is null)
            return null;

        return tree
            .Where(e => e.TargetType is TreeEntryTargetType.Tree or TreeEntryTargetType.Blob)
            .Select(e =>
            {
                var itemPath = normalizedPath.Length == 0 ? e.Name : $"{normalizedPath}/{e.Name}";
                return e.TargetType == TreeEntryTargetType.Tree
                    ? new GitTreeItem(e.Name, itemPath, GitTreeItemType.Folder, null)
                    : new GitTreeItem(e.Name, itemPath, GitTreeItemType.File, ((Blob)e.Target).Size);
            })
            .OrderBy(i => i.Type == GitTreeItemType.Folder ? 0 : 1)
            .ThenBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public GitFile? GetFile(Guid projectId, string repositoryName, string branch, string path)
    {
        var normalizedPath = NormalizePath(path);
        if (normalizedPath.Length == 0)
        {
            return null;
        }

        using var repo = Open(projectId, repositoryName);
        var commit = repo is null ? null : ResolveBranchTip(repo, branch);

        if (commit?[normalizedPath]?.Target is not Blob blob)
        {
            return null;
        }

        var size = blob.Size;

        if (blob.IsBinary)
        {
            return new GitFile(normalizedPath, null, true, size);
        }

        if (size > MaxPreviewBytes)
        {
            return new GitFile(normalizedPath, null, false, size);
        }

        return new GitFile(normalizedPath, blob.GetContentText(), false, size);
    }

    public GitCommitsPage? GetCommits(Guid projectId, string repositoryName, string branch, int page, int pageSize,
        CancellationToken ct = default)
    {
        using var repo = Open(projectId, repositoryName);
        var tip = repo is null ? null : ResolveBranchTip(repo, branch);
        if (tip is null)
            return null;

        var filter = new CommitFilter
        {
            IncludeReachableFrom = tip,
            SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time
        };

        var items = new List<GitCommit>(pageSize + 1);

        foreach (var commit in repo!.Commits.QueryBy(filter).Skip((page - 1) * pageSize).Take(pageSize + 1))
        {
            ct.ThrowIfCancellationRequested();

            items.Add(new GitCommit(
                commit.Sha,
                commit.Message.TrimEnd(),
                commit.Author.Name,
                string.IsNullOrWhiteSpace(commit.Author.Email) ? null : commit.Author.Email,
                commit.Author.When));
        }

        var hasMore = items.Count > pageSize;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        return new GitCommitsPage(items, hasMore);
    }

    private static Repository? Open(Guid projectId, string repositoryName)
    {
        if (!GitRepositoryPaths.IsValidName(repositoryName))
            return null;

        var gitPath = GitRepositoryPaths.GetRepositoryPath(projectId, repositoryName);
        return Repository.IsValid(gitPath) ? new Repository(gitPath) : null;
    }

    private static Commit? ResolveBranchTip(Repository repo, string branch)
    {
        if (string.IsNullOrWhiteSpace(branch))
            return null;

        try
        {
            var reference = repo.Branches[branch];
            return reference is null || reference.IsRemote ? null : reference.Tip;
        }
        catch (LibGit2SharpException)
        {
            return null;
        }
    }

    private static string NormalizePath(string? path) =>
        (path ?? string.Empty).Replace('\\', '/').Trim('/');
}
