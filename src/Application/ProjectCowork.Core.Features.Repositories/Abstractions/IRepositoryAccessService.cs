namespace ProjectCowork.Core.Features.Repositories.Abstractions;

public sealed record RepositoryLocation(Guid ProjectId, string Name);

public interface IRepositoryAccessService
{
    Task<RepositoryLocation> GetViewableRepositoryAsync(Guid repositoryId, CancellationToken ct = default);
}
