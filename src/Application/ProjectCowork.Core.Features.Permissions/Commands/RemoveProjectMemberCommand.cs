using DispatchR.Abstractions.Send;
using ProjectCowork.Core.Features.Permissions.Extensions;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Permissions.Commands;

public record RemoveProjectMemberCommand : IRequest<RemoveProjectMemberCommand, Task>
{
    public required Guid ProjectId { get; init; }
    public required Guid UserId { get; init; }
}

internal class RemoveProjectMemberCommandHandler : IRequestHandler<RemoveProjectMemberCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public RemoveProjectMemberCommandHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task Handle(RemoveProjectMemberCommand request, CancellationToken ct)
    {
        await _permissionService.EnsureProjectPermissionAsync(request.ProjectId, ResourceType.Permissions, ActionType.Delete, ct);

        var project = await _dbContext.GetProjectWithAccessGraphAsync(request.ProjectId, ct);

        new ProjectAggregate(project).RemoveMember(request.UserId);

        await _dbContext.SaveChangesAsync(ct);
    }
}
