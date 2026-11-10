using System.Runtime.CompilerServices;
using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Management.Models;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Management.Queries;

public class GetProjectSprintsQuery : IStreamRequest<GetProjectSprintsQuery, SprintDto>
{
    public required Guid ProjectId { get; init; }
}

internal class GetProjectSprintsQueryHandler : IStreamRequestHandler<GetProjectSprintsQuery, SprintDto>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public GetProjectSprintsQueryHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async IAsyncEnumerable<SprintDto> Handle(GetProjectSprintsQuery request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await _permissionService.EnsureProjectPermissionAsync(request.ProjectId, ResourceType.Management,
            ActionType.View, cancellationToken);

        var sprints = _dbContext.Sprints
            .Where(x => x.ProjectId == request.ProjectId)
            .Select(x => new SprintDto
            {
                Id = x.Id,
                Name = x.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate
            })
            .AsAsyncEnumerable();

        await foreach (var sprint in sprints.WithCancellation(cancellationToken))
        {
            yield return sprint;
        }
    }
}
