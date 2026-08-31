using ProjectCowork.Domain.Models.Posting;

namespace ProjectCowork.Domain.Models.Attachments;

public class AttachmentEntity : BaseTrackedEntity
{
    public Guid Id  { get; }
    
    public required string FileName { get; init; }
    public required string PersistedFileName { get; init; }
    public required long SizeInBytes { get; init; }
    public string? BlobUrl { get; set; }
    
    public Guid? ProjectApplicationId { get; set; }
    public ProjectApplicationEntity? ProjectApplication { get; set; }
}