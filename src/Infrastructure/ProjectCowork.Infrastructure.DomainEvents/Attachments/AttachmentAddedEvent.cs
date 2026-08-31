using DispatchR.Abstractions.Notification;

namespace ProjectCowork.Infrastructure.DomainEvents.Attachments;

public record AttachmentAddedEvent : INotification
{
   public required Stream ContentStream { get; init; }
   public required string FileName { get; init; }
   public required string ContentType { get; init; }
}

internal class AttachmentAddedEventHandler : INotificationHandler<AttachmentAddedEvent>
{
    public ValueTask Handle(AttachmentAddedEvent request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}