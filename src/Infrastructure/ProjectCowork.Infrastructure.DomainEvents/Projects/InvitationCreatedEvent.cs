using DispatchR.Abstractions.Notification;

namespace ProjectCowork.Infrastructure.DomainEvents.Projects;

public record InvitationCreatedEvent : INotification
{
    public required Guid InvitationId { get; init; }
}