using DispatchR.Abstractions.Notification;

namespace ProjectCowork.Infrastructure.DomainEvents.Attachments;

public class AttachmentAddedEvent : INotification
{
    // TODO
}

internal class AttachmentAddedEventHandler : INotificationHandler<AttachmentAddedEvent>
{
    public ValueTask Handle(AttachmentAddedEvent request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}