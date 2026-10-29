using System.Runtime.CompilerServices;
using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Permissions.Models;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Permissions.Queries;

/// <summary>
/// Członkowie projektu. Właściciel jest zawsze pierwszy, z <c>IsOwner = true</c> i bez roli.
/// </summary>
public record GetProjectMembersQuery : IStreamRequest<GetProjectMembersQuery, ProjectMemberDto>
{
    public required Guid ProjectId { get; init; }
}

internal class GetProjectMembersQueryHandler : IStreamRequestHandler<GetProjectMembersQuery, ProjectMemberDto>
{
    private const string OwnerRoleName = "Owner";

    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public GetProjectMembersQueryHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async IAsyncEnumerable<ProjectMemberDto> Handle(GetProjectMembersQuery request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await _permissionService.EnsureProjectPermissionAsync(request.ProjectId, ResourceType.Permissions,
            ActionType.View, cancellationToken);

        var owner = await _dbContext.Projects
            .AsNoTracking()
            .Where(x => x.Id == request.ProjectId)
            .Select(x => new ProjectMemberDto
            {
                UserId = x.OwnerId,
                UserName = x.Owner.UserName ?? string.Empty,
                Email = x.Owner.Email ?? string.Empty,
                RoleId = null,
                RoleName = OwnerRoleName,
                IsOwner = true
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (owner is null)
        {
            yield break;
        }

        yield return owner;

        var members = _dbContext.ProjectMembers
            .AsNoTracking()
            .Where(x => x.ProjectId == request.ProjectId)
            .OrderBy(x => x.User.UserName)
            .Select(x => new ProjectMemberDto
            {
                UserId = x.UserId,
                UserName = x.User.UserName ?? string.Empty,
                Email = x.User.Email ?? string.Empty,
                RoleId = x.ProjectRoleId,
                RoleName = x.ProjectRole.Name,
                IsOwner = false
            })
            .AsAsyncEnumerable();

        await foreach (var member in members.WithCancellation(cancellationToken))
        {
            yield return member;
        }
    }
}
