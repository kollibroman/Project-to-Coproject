using DispatchR.Abstractions.Notification;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Infrastructure.DomainEvents.Management;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Management.EventHandlers;

internal class SprintDeletedEventHandler : INotificationHandler<SprintDeletedEvent>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public SprintDeletedEventHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask Handle(SprintDeletedEvent request, CancellationToken cancellationToken)
    {
        var tasks = await _dbContext.Tasks
            .Where(x => x.SprintId == request.SprintId)
            .ToListAsync(cancellationToken);

        // TODO: Moze nie dzialac
        foreach (var task in tasks)
        {
            new TaskAggregate(task).MoveToBacklog();
        }
    }
}
