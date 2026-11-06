using DispatchR.Abstractions.Notification;

namespace ProjectCowork.Infrastructure.DomainEvents.Projects;

public record InvitationAcceptedEvent : INotification
{
    public required Guid InvitationId { get; set; }
}