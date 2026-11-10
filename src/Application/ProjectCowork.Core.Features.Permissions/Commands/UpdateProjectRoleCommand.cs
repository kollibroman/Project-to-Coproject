using DispatchR.Abstractions.Send;
using ProjectCowork.Core.Features.Permissions.Extensions;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Permissions.Commands;

/// <summary>
/// Zmienia nazwę roli i zastępuje cały zestaw jej uprawnień.
/// </summary>
public record UpdateProjectRoleCommand : IRequest<UpdateProjectRoleCommand, Task>
{
    public required Guid ProjectId { get; init; }
    public required Guid RoleId { get; init; }
    public required string Name { get; init; }
    public required bool IsDefault { get; init; }
    public required IReadOnlyCollection<int> PermissionIds { get; init; }
}

internal class UpdateProjectRoleCommandHandler : IRequestHandler<UpdateProjectRoleCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public UpdateProjectRoleCommandHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task Handle(UpdateProjectRoleCommand request, CancellationToken ct)
    {
        await _permissionService.EnsureProjectPermissionAsync(request.ProjectId, ResourceType.Permissions, ActionType.Update, ct);

        var project = await _dbContext.GetProjectWithAccessGraphAsync(request.ProjectId, ct);
        var permissions = await _dbContext.GetPermissionsByIdsAsync(request.PermissionIds, ct);

        new ProjectAggregate(project).UpdateRole(request.RoleId, request.Name, request.IsDefault, permissions);

        await _dbContext.SaveChangesAsync(ct);
    }
}
