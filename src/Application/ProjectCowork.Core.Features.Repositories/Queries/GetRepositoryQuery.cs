using System.Net;
using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Repositories.Models;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Repositories.Queries;

public record GetRepositoryQuery : IRequest<GetRepositoryQuery, Task<RepositoryDto>>
{
    public required Guid RepositoryId { get; init; }
}

internal class GetRepositoryQueryHandler : IRequestHandler<GetRepositoryQuery, Task<RepositoryDto>>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public GetRepositoryQueryHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task<RepositoryDto> Handle(GetRepositoryQuery request, CancellationToken cancellationToken)
    {
        var result = await _dbContext.Repositories
            .Where(x => x.Id == request.RepositoryId)
            .Select(x => new
            {
                x.ProjectId,
                Repository = new RepositoryDto
                {
                    Name = x.RepositoryName,
                    Id = x.Id,
                    RepoUrl = x.RepoPath
                }
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            throw new EntityNotFoundException($"Repository {request.RepositoryId} does not exist", HttpStatusCode.NotFound);
        }

        await _permissionService.EnsureProjectPermissionAsync(result.ProjectId, ResourceType.Repositories,
            ActionType.View, cancellationToken);

        return result.Repository;
    }
}
