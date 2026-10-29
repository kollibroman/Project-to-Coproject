using System.Runtime.CompilerServices;
using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Management.Models;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Management.Queries;

public record GetTasksFromSprintQuery : IStreamRequest<GetTasksFromSprintQuery, TaskDto>
{
    public required Guid SprintId { get; init; }
}

internal class GetTasksFromSprintQueryHandler : IStreamRequestHandler<GetTasksFromSprintQuery, TaskDto>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public GetTasksFromSprintQueryHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async IAsyncEnumerable<TaskDto> Handle(GetTasksFromSprintQuery request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var projectId = await _dbContext.Sprints
            .Where(x => x.Id == request.SprintId)
            .Select(x => (Guid?)x.ProjectId)
            .FirstOrDefaultAsync(cancellationToken);

        // Nieistniejący sprint daje pustą listę, tak jak wcześniej.
        if (projectId is null)
        {
            yield break;
        }

        await _permissionService.EnsureProjectPermissionAsync(projectId.Value, ResourceType.Management,
            ActionType.View, cancellationToken);

        var tasks = _dbContext.Tasks
            .Where(x => x.SprintId == request.SprintId)
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
