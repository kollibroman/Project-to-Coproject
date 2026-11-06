using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Permissions.Extensions;
using ProjectCowork.Core.Features.Permissions.Models;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Permissions.Queries;

public class GetPermissionsQuery : IStreamRequest<GetPermissionsQuery, PermissionDto>
{
}

internal class GetPermissionsQueryHandler : IStreamRequestHandler<GetPermissionsQuery, PermissionDto>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public GetPermissionsQueryHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IAsyncEnumerable<PermissionDto> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        return _dbContext.FeatureActions
            .AsNoTracking()
            .OrderBy(x => x.Feature.Name)
            .ThenBy(x => x.Action.Name)
            .Select(PermissionDbContextExtensions.ToPermissionDto)
            .AsAsyncEnumerable();
    }
}
