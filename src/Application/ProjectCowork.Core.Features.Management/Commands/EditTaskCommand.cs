using System.Net;
using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Enums.Management;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;
using TaskStatus = ProjectCowork.Domain.Enums.Management.TaskStatus;

namespace ProjectCowork.Core.Features.Management.Commands;

public record EditTaskCommand : IRequest<EditTaskCommand, Task>
{
    public required Guid TaskId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required TaskStatus Status { get; init; }
    public required TaskPriority Priority { get; init; }

    /// <summary>
    /// Null moves the task to the project backlog.
    /// </summary>
    public Guid? SprintId { get; init; }

    /// <summary>
    /// Null unassigns the task.
    /// </summary>
    public Guid? AssignedUserId { get; init; }
}

internal class EditTaskCommandHandler : IRequestHandler<EditTaskCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public EditTaskCommandHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task Handle(EditTaskCommand request, CancellationToken ct)
    {
        var task = await _dbContext.Tasks
            .FirstOrDefaultAsync(x => x.Id == request.TaskId, ct);

        if (task is null)
        {
            throw new EntityNotFoundException($"Task {request.TaskId} does not exist");
        }

        await _permissionService.EnsureProjectPermissionAsync(task.ProjectId, ResourceType.Management, ActionType.Update, ct);

        var aggregate = new TaskAggregate(task);

        aggregate.SetName(request.Name);
        aggregate.SetDescription(request.Description);
        aggregate.SetPriority(request.Priority);
        aggregate.SetStatus(request.Status);

        if (request.SprintId is null)
        {
            aggregate.MoveToBacklog();
        }
        else if (request.SprintId != task.SprintId)
        {
            var sprint = await _dbContext.Sprints
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.SprintId, ct);

            if (sprint is null)
            {
                throw new EntityNotFoundException($"Sprint {request.SprintId} does not exist", HttpStatusCode.BadRequest);
            }

            aggregate.MoveToSprint(sprint);
        }

        if (request.AssignedUserId is null)
        {
            aggregate.Unassign();
        }
        else if (request.AssignedUserId != task.AssignedUserId)
        {
            var userExists = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.AssignedUserId, ct);

            if (!userExists)
            {
                throw new EntityNotFoundException($"User {request.AssignedUserId} does not exist", HttpStatusCode.BadRequest);
            }

            aggregate.AssignUser(request.AssignedUserId.Value);
        }

        await _dbContext.SaveChangesAsync(ct);
    }
}
