using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Models;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Queries;

public record GetProjectQuery : IRequest<GetProjectQuery, Task<ProjectModel>>
{
    public required Guid ProjectId { get; init; }
}

internal class GetProjectQueryHandler : IRequestHandler<GetProjectQuery, Task<ProjectModel>>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public GetProjectQueryHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task<ProjectModel> Handle(GetProjectQuery request, CancellationToken ct)
    {
        await _permissionService.EnsureProjectPermissionAsync(request.ProjectId, ResourceType.Projects, ActionType.View, ct);

        var project = await _dbContext.Projects
            .Where(x => x.Id == request.ProjectId)
            .Select(x => new ProjectModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                OwnerId = x.OwnerId
            })
            .FirstOrDefaultAsync(ct);

        if (project is null)
        {
            throw new EntityNotFoundException("Project not found");
        }

        return project;
    }
}
