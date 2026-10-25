using DispatchR.Abstractions.Notification;

namespace ProjectCowork.Infrastructure.DomainEvents.Management;

public sealed record SprintDeletedEvent : INotification
{
    public required Guid SprintId { get; init; }
    public required Guid ProjectId { get; init; }
}
