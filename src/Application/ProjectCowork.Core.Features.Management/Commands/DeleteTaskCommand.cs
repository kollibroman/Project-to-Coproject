using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Exceptions.Common;
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

    public DeleteTaskCommandHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteTaskCommand request, CancellationToken ct)
    {
        var task = await _dbContext.Tasks
            .FirstOrDefaultAsync(x => x.Id == request.TaskId, ct);

        if (task is null)
        {
            throw new EntityNotFoundException($"Task {request.TaskId} does not exist");
        }

        var aggregate = new TaskAggregate(task);

        aggregate.DeleteTask();

        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync(ct);
    }
}
