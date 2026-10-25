using DispatchR;
using DispatchR.Abstractions.Notification;

namespace ProjectCowork.Infrastructure.Domain.Abstractions;

public abstract class AggregateRoot
{
    private readonly List<INotification> _domainEvents = [];
    private readonly List<Func<IMediator, CancellationToken, Task>> _publishers = [];

    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent<TEvent>(TEvent domainEvent) where TEvent : class, INotification
    {
        _domainEvents.Add(domainEvent);
        _publishers.Add(async (mediator, ct) => await mediator.Publish(domainEvent, ct));
    }

    internal IReadOnlyList<Func<IMediator, CancellationToken, Task>> DequeueDomainEvents()
    {
        var publishers = _publishers.ToList();

        _publishers.Clear();
        _domainEvents.Clear();

        return publishers;
    }
}
