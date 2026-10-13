namespace ProjectCowork.Infrastructure.Integrations.Git.Abstractions;

public interface IGitServerService
{
    Task<string> UploadAndCreateRemoteRepositoryAsync(Guid projectId, string name, string gitAction, CancellationToken ct);
}