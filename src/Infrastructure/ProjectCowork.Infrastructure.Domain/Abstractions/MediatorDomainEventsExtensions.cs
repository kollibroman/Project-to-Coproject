using DispatchR;

namespace ProjectCowork.Infrastructure.Domain.Abstractions;

public static class MediatorDomainEventsExtensions
{
    public static async Task PublishDomainEventsAsync(this IMediator mediator, AggregateRoot aggregate,
        CancellationToken ct)
    {
        foreach (var publish in aggregate.DequeueDomainEvents())
        {
            await publish(mediator, ct);
        }
    }

    public static async Task PublishDomainEventsAsync(this IMediator mediator, IEnumerable<AggregateRoot> aggregates,
        CancellationToken ct)
    {
        foreach (var aggregate in aggregates)
        {
            await mediator.PublishDomainEventsAsync(aggregate, ct);
        }
    }
}
