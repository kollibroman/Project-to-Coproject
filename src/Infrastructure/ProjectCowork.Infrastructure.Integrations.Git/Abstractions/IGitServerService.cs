namespace ProjectCowork.Infrastructure.Integrations.Git.Abstractions;

public interface IGitServerService
{
    Task<string> UploadAndCreateRemoteRepositoryAsync(Guid repositoryId, string name, string gitAction, CancellationToken ct);
}