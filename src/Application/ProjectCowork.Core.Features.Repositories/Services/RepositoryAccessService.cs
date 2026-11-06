using System.Net;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Repositories.Abstractions;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Repositories.Services;

internal class RepositoryAccessService : IRepositoryAccessService
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public RepositoryAccessService(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task<RepositoryLocation> GetViewableRepositoryAsync(Guid repositoryId, CancellationToken ct = default)
    {
        var repository = await _dbContext.Repositories
            .Where(x => x.Id == repositoryId)
            .Select(x => new RepositoryLocation(x.ProjectId, x.RepositoryName))
            .FirstOrDefaultAsync(ct);

        if (repository is null)
        {
            throw new EntityNotFoundException($"Repository {repositoryId} does not exist", HttpStatusCode.NotFound);
        }

        await _permissionService.EnsureProjectPermissionAsync(repository.ProjectId, ResourceType.Repositories,
            ActionType.View, ct);

        return repository;
    }
}
