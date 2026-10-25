using System.Net;
using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Enums.Management;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Domain.Models.Management;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;
using TaskStatus = ProjectCowork.Domain.Enums.Management.TaskStatus;

namespace ProjectCowork.Core.Features.Management.Commands;

public record AddTaskCommand : IRequest<AddTaskCommand, Task>
{
    public required Guid ProjectId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required TaskPriority Priority { get; init; }
    public TaskStatus Status { get; init; } = TaskStatus.Todo;
    public Guid? SprintId { get; init; }
    public Guid? AssignedUserId { get; init; }
}

internal class AddTaskCommandHandler : IRequestHandler<AddTaskCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public AddTaskCommandHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(AddTaskCommand request, CancellationToken ct)
    {
        var project = await _dbContext.Projects
            .Include(x => x.Tasks)
            .FirstOrDefaultAsync(x => x.Id == request.ProjectId, ct);

        if (project is null)
        {
            throw new EntityNotFoundException($"Project {request.ProjectId} does not exist", HttpStatusCode.BadRequest);
        }

        SprintEntity? sprint = null;

        if (request.SprintId is not null)
        {
            sprint = await _dbContext.Sprints
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.SprintId, ct);

            if (sprint is null)
            {
                throw new EntityNotFoundException($"Sprint {request.SprintId} does not exist", HttpStatusCode.BadRequest);
            }
        }

        if (request.AssignedUserId is not null)
        {
            var userExists = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.AssignedUserId, ct);

            if (!userExists)
            {
                throw new EntityNotFoundException($"User {request.AssignedUserId} does not exist", HttpStatusCode.BadRequest);
            }
        }

        var entity = TaskAggregate.Create(project, request.Name, request.Description, request.Priority,
            request.Status, sprint, request.AssignedUserId);

        _dbContext.Tasks.Add(entity);
        await _dbContext.SaveChangesAsync(ct);
    }
}
