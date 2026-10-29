using System.Linq.Expressions;
using System.Net;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Permissions.Models;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Domain.Models.Permissions;
using ProjectCowork.Domain.Models.Project;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Permissions.Extensions;

internal static class PermissionDbContextExtensions
{
    public static readonly Expression<Func<FeatureActionEntity, PermissionDto>> ToPermissionDto = x => new PermissionDto
    {
        Id = x.Id,
        Resource = Enum.Parse<ResourceType>(x.Feature.Name),
        Action = Enum.Parse<ActionType>(x.Action.Name)
    };

    public static async Task<ProjectEntity> GetProjectWithAccessGraphAsync(this ProjectCoworkDbContext dbContext,
        Guid projectId, CancellationToken ct)
    {
        return await dbContext.Projects
                   .IncludeAccessGraph()
                   .FirstOrDefaultAsync(x => x.Id == projectId, ct)
               ?? throw new EntityNotFoundException($"Project {projectId} does not exist", HttpStatusCode.NotFound);
    }

    public static async Task<List<FeatureActionEntity>> GetPermissionsByIdsAsync(this ProjectCoworkDbContext dbContext,
        IReadOnlyCollection<int> permissionIds, CancellationToken ct)
    {
        var ids = permissionIds.Distinct().ToList();

        var permissions = await dbContext.FeatureActions
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(ct);

        if (permissions.Count != ids.Count)
        {
            var missing = ids.Except(permissions.Select(x => x.Id));
            throw new EntityNotFoundException($"Unknown permissions: {string.Join(", ", missing)}", HttpStatusCode.BadRequest);
        }

        return permissions;
    }
}
