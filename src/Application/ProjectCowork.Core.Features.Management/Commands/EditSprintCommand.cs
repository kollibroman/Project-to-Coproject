using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Management.Commands;

public record EditSprintCommand : IRequest<EditSprintCommand, Task>
{
    public required Guid SprintId { get; init; }
    public required string Name { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
}

internal class EditSprintCommandHandler : IRequestHandler<EditSprintCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public EditSprintCommandHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(EditSprintCommand request, CancellationToken ct)
    {
        var sprint = await _dbContext.Sprints
            .FirstOrDefaultAsync(x => x.Id == request.SprintId, ct);

        if (sprint is null)
        {
            throw new EntityNotFoundException($"Sprint {request.SprintId} does not exist");
        }

        var aggregate = new SprintAggregate(sprint);

        aggregate.SetName(request.Name);
        aggregate.SetDates(request.StartDate, request.EndDate);

        await _dbContext.SaveChangesAsync(ct);
    }
}
