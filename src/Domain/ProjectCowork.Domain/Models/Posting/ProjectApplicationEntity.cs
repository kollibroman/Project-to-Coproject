using ProjectCowork.Domain.Models.Attachments;
using ProjectCowork.Domain.Models.Users;

namespace ProjectCowork.Domain.Models.Posting;

public class ProjectApplicationEntity : BaseTrackedEntity
{
    public Guid Id { get; set; }
    
    public required string Description { get; set; }
    
    public required Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;
    
    public required Guid ProjectPostingId { get; set; }
    public ProjectPostingEntity ProjectPosting { get; set; } = null!;
    
    public ICollection<AttachmentEntity> Attachments { get; set; } = [];
}