using System.Text.RegularExpressions;

namespace ProjectCowork.Infrastructure.Integrations.Git.Internal;

/// <summary>
/// Single source of truth for the on-disk layout: Repos/{projectId}/{repositoryName}.git
/// </summary>
internal static partial class GitRepositoryPaths
{
    public static string ReposRoot => Path.Combine(Directory.GetCurrentDirectory(), "Repos");

    public static string GetProjectRoot(Guid projectId) =>
        Path.Combine(ReposRoot, projectId.ToString());

    public static string GetRepositoryPath(Guid projectId, string repositoryName)
    {
        if (!IsValidName(repositoryName))
            throw new ArgumentException($"Invalid repository name '{repositoryName}'.", nameof(repositoryName));

        return Path.Combine(GetProjectRoot(projectId), $"{repositoryName}.git");
    }

    // Letters, digits, '.', '_', '-'; no leading dot. Blocks "../" and other path tricks.
    public static bool IsValidName(string repositoryName) =>
        !string.IsNullOrWhiteSpace(repositoryName) && NameRegex().IsMatch(repositoryName);

    [GeneratedRegex(@"^[A-Za-z0-9_\-][A-Za-z0-9._\-]{0,99}$")]
    private static partial Regex NameRegex();
}
