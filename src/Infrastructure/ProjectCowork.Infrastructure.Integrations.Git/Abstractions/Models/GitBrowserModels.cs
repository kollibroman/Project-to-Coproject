namespace ProjectCowork.Infrastructure.Integrations.Git.Abstractions.Models;

public enum GitTreeItemType
{
    Folder,
    File
}

public sealed record GitTreeItem(string Name, string Path, GitTreeItemType Type, long? Size);

public sealed record GitFile(string Path, string? Content, bool IsBinary, long Size);

public sealed record GitBranch(string Name, bool IsDefault);

public sealed record GitCommit(string Sha, string Message, string AuthorName, string? AuthorEmail, DateTimeOffset Date);

public sealed record GitCommitsPage(IReadOnlyList<GitCommit> Items, bool HasMore);
