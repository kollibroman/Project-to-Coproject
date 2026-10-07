namespace ProjectCowork.Infrastructure.Integrations.Git.Abstractions;

public interface IGitServerService
{
    Task<string> UploadAndCreateRemoteRepositoryAsync(string name, CancellationToken ct);
}