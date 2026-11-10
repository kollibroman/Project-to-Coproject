using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Models.Project;

namespace ProjectCowork.Infrastructure.Domain.Aggregates;

public static class ProjectAggregateQueryExtensions
{
    public static IQueryable<ProjectEntity> IncludeAccessGraph(this IQueryable<ProjectEntity> query)
    {
        return query
            .Include(x => x.ProjectRoles)
                .ThenInclude(x => x.FeatureActions)
            .Include(x => x.ProjectMembers)
            .AsSplitQuery();
    }
}
