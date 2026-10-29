using DispatchR.Abstractions.Send;
using ProjectCowork.Core.Features.Permissions.Extensions;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Permissions.Commands;

public record CreateProjectRoleCommand : IRequest<CreateProjectRoleCommand, Task>
{
    public required Guid ProjectId { get; init; }
    public required string Name { get; init; }
    public required bool IsDefault { get; init; }
    public required IReadOnlyCollection<int> PermissionIds { get; init; }
}

internal class CreateProjectRoleCommandHandler : IRequestHandler<CreateProjectRoleCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public CreateProjectRoleCommandHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task Handle(CreateProjectRoleCommand request, CancellationToken ct)
    {
        await _permissionService.EnsureProjectPermissionAsync(request.ProjectId, ResourceType.Permissions, ActionType.Create, ct);

        var project = await _dbContext.GetProjectWithAccessGraphAsync(request.ProjectId, ct);
        var permissions = await _dbContext.GetPermissionsByIdsAsync(request.PermissionIds, ct);

        new ProjectAggregate(project).AddRole(request.Name, request.IsDefault, permissions);

        await _dbContext.SaveChangesAsync(ct);
    }
}
