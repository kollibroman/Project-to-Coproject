using System.Runtime.CompilerServices;
using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Permissions.Models;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Permissions.Queries;

public record GetProjectRolesQuery : IStreamRequest<GetProjectRolesQuery, ProjectRoleDto>
{
    public required Guid ProjectId { get; init; }
}

internal class GetProjectRolesQueryHandler : IStreamRequestHandler<GetProjectRolesQuery, ProjectRoleDto>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public GetProjectRolesQueryHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async IAsyncEnumerable<ProjectRoleDto> Handle(GetProjectRolesQuery request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Sprawdzenie przed pierwszym elementem, żeby błąd 403 poszedł zanim zacznie się odpowiedź.
        await _permissionService.EnsureProjectPermissionAsync(request.ProjectId, ResourceType.Permissions,
            ActionType.View, cancellationToken);

        var roles = _dbContext.ProjectRoles
            .AsNoTracking()
            .Where(x => x.ProjectId == request.ProjectId)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .Select(x => new ProjectRoleDto
            {
                Id = x.Id,
                Name = x.Name,
                IsDefault = x.IsDefault,
                PermissionIds = x.FeatureActions.Select(fa => fa.Id).ToList(),
                MemberCount = _dbContext.ProjectMembers.Count(m => m.ProjectRoleId == x.Id)
            })
            .AsAsyncEnumerable();

        await foreach (var role in roles.WithCancellation(cancellationToken))
        {
            yield return role;
        }
    }
}
