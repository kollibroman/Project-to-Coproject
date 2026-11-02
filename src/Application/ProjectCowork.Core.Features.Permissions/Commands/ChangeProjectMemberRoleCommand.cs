using DispatchR.Abstractions.Send;
using ProjectCowork.Core.Features.Permissions.Extensions;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Permissions.Commands;

public record ChangeProjectMemberRoleCommand : IRequest<ChangeProjectMemberRoleCommand, Task>
{
    public required Guid ProjectId { get; init; }
    public required Guid UserId { get; init; }
    public required Guid RoleId { get; init; }
}

internal class ChangeProjectMemberRoleCommandHandler : IRequestHandler<ChangeProjectMemberRoleCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public ChangeProjectMemberRoleCommandHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task Handle(ChangeProjectMemberRoleCommand request, CancellationToken ct)
    {
        await _permissionService.EnsureProjectPermissionAsync(request.ProjectId, ResourceType.Permissions, ActionType.Update, ct);

        var project = await _dbContext.GetProjectWithAccessGraphAsync(request.ProjectId, ct);

        new ProjectAggregate(project).ChangeMemberRole(request.UserId, request.RoleId);

        await _dbContext.SaveChangesAsync(ct);
    }
}
