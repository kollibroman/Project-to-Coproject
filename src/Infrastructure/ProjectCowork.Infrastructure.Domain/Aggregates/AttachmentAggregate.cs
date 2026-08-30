using DispatchR;
using ProjectCowork.Domain.Models.Attachments;
using ProjectCowork.Infrastructure.DomainEvents.Attachments;

namespace ProjectCowork.Infrastructure.Domain.Aggregates;

public class AttachmentAggregate
{
    private AttachmentEntity AttachmentEntity { get; }
    
    public AttachmentAggregate(AttachmentEntity attachmentEntity)
    {
        AttachmentEntity = attachmentEntity;
    }

    public static AttachmentEntity Create(string fileName, string persistedFileName, int sizeInBytes)
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

    public async Task SetBlobAsync(IMediator mediator, CancellationToken ct)
    {
        await mediator.Publish(new AttachmentAddedEvent
        {

        }, ct);
    }
}