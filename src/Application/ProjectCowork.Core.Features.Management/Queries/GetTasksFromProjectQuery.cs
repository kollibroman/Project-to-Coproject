using System.Runtime.CompilerServices;
using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Management.Models;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Management.Queries;

public record GetTasksFromProjectQuery : IStreamRequest<GetTasksFromProjectQuery, TaskDto>
{
    public required Guid ProjectId { get; init; }
}

internal class GetTasksFromProjectQueryHandler : IStreamRequestHandler<GetTasksFromProjectQuery, TaskDto>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public GetTasksFromProjectQueryHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async IAsyncEnumerable<TaskDto> Handle(GetTasksFromProjectQuery request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await _permissionService.EnsureProjectPermissionAsync(request.ProjectId, ResourceType.Management,
            ActionType.View, cancellationToken);

        var tasks = _dbContext.Tasks
            .Where(x => x.ProjectId == request.ProjectId)
            .Select(x => new TaskDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Status = x.Status,
                Priority = x.Priority,
                FinishedDate = x.FinishedDate,
                SprintId = x.SprintId,
                AssignedUserId = x.AssignedUserId
            })
            .AsAsyncEnumerable();

        await foreach (var task in tasks.WithCancellation(cancellationToken))
        {
            yield return task;
        }
    }
}
