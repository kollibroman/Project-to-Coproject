using ProjectCowork.Domain.Models.Attachments;
using ProjectCowork.Infrastructure.Domain.Abstractions;
using ProjectCowork.Infrastructure.DomainEvents.Attachments;

namespace ProjectCowork.Infrastructure.Domain.Aggregates;

public class AttachmentAggregate : AggregateRoot
{
    private AttachmentEntity AttachmentEntity { get; }
    private Stream AttachmentStream { get; }
    private string ContentType { get; }
    
    public AttachmentAggregate(AttachmentEntity attachmentEntity, Stream attachmentStream, string contentType)
    {
        ArgumentNullException.ThrowIfNull(attachmentEntity);
        AttachmentEntity = attachmentEntity;
        AttachmentStream = attachmentStream;
        ContentType = contentType;
    }

    public static AttachmentEntity Create(string fileName, string persistedFileName, long sizeInBytes)
    {
        //TODO: validation and other things
        
        return new AttachmentEntity
        {
            FileName = fileName,
            PersistedFileName = persistedFileName,
            SizeInBytes = sizeInBytes
        };
    }

    public void SetProjectApplicationId(Guid projectApplicationId)
    {
        AttachmentEntity.ProjectApplicationId = projectApplicationId;
    }

    public void ScheduleBlobUpload()
    {
        RaiseDomainEvent(new AttachmentAddedEvent
        {
            ContentStream = AttachmentStream,
            FileName = AttachmentEntity.FileName,
            ContentType = ContentType,
        });
    }
}
