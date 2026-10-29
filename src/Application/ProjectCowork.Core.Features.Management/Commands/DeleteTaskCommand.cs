using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Management.Commands;

public record DeleteTaskCommand : IRequest<DeleteTaskCommand, Task>
{
    public required Guid TaskId { get; init; }
}

internal class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public DeleteTaskCommandHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task Handle(DeleteTaskCommand request, CancellationToken ct)
    {
        var task = await _dbContext.Tasks
            .FirstOrDefaultAsync(x => x.Id == request.TaskId, ct);

        if (task is null)
        {
            throw new EntityNotFoundException($"Task {request.TaskId} does not exist");
        }

        await _permissionService.EnsureProjectPermissionAsync(task.ProjectId, ResourceType.Management, ActionType.Delete, ct);

        var aggregate = new TaskAggregate(task);

        aggregate.DeleteTask();

        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync(ct);
    }
}
