using ProjectCowork.Domain.Models.Project;

namespace ProjectCowork.Domain.Models.Posting;

public class ProjectPostingEntity : BaseTrackedEntity
{
    public Guid Id { get; }
    
    public required string JobDescription { get; set; }
    public required string ProjectDescription { get; set; }
    
    public required Guid ProjectId { get; set; }
    public ProjectEntity Project { get; set; } = null!;

    public ICollection<ProjectApplicationEntity> Applications { get; set; } = [];
    
    public bool IsActive { get; set; }
}