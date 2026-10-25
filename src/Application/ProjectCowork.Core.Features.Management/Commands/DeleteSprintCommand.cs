using DispatchR;
using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Domain.Abstractions;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Management.Commands;

public record DeleteSprintCommand : IRequest<DeleteSprintCommand, Task>
{
    public required Guid SprintId { get; init; }
}

internal class DeleteSprintCommandHandler : IRequestHandler<DeleteSprintCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IMediator _mediator;

    public DeleteSprintCommandHandler(ProjectCoworkDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task Handle(DeleteSprintCommand request, CancellationToken ct)
    {
        var sprint = await _dbContext.Sprints
            .FirstOrDefaultAsync(x => x.Id == request.SprintId, ct);

        if (sprint is null)
        {
            throw new EntityNotFoundException($"Sprint {request.SprintId} does not exist");
        }

        var aggregate = new SprintAggregate(sprint);

        aggregate.DeleteSprint();

        await _mediator.PublishDomainEventsAsync(aggregate, ct);

        _dbContext.Sprints.Remove(sprint);
        await _dbContext.SaveChangesAsync(ct);
    }
}
