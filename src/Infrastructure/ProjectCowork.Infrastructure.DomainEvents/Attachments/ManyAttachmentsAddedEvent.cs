using DispatchR.Abstractions.Notification;

namespace ProjectCowork.Infrastructure.DomainEvents.Attachments;

public sealed record ManyAttachmentsAddedEvent : INotification
{
    // TODO
}

internal sealed class ManyAttachmentsAddedEventHandler : INotificationHandler<ManyAttachmentsAddedEvent>
{
    public ValueTask Handle(ManyAttachmentsAddedEvent request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}