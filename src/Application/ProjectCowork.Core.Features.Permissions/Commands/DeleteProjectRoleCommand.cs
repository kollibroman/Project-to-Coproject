using DispatchR.Abstractions.Send;
using ProjectCowork.Core.Features.Permissions.Extensions;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Permissions.Commands;

public record DeleteProjectRoleCommand : IRequest<DeleteProjectRoleCommand, Task>
{
    public required Guid ProjectId { get; init; }
    public required Guid RoleId { get; init; }
}

internal class DeleteProjectRoleCommandHandler : IRequestHandler<DeleteProjectRoleCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public DeleteProjectRoleCommandHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task Handle(DeleteProjectRoleCommand request, CancellationToken ct)
    {
        await _permissionService.EnsureProjectPermissionAsync(request.ProjectId, ResourceType.Permissions, ActionType.Delete, ct);

        var project = await _dbContext.GetProjectWithAccessGraphAsync(request.ProjectId, ct);

        new ProjectAggregate(project).RemoveRole(request.RoleId);

        await _dbContext.SaveChangesAsync(ct);
    }
}
