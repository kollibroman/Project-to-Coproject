using ProjectCowork.Domain.Models.Project;

namespace ProjectCowork.Domain.Models.Management;

public class SprintEntity : BaseTrackedEntity
{
    public Guid Id { get; }

    public required string Name { get; set; }

    public required DateOnly StartDate { get; set; }
    public required DateOnly EndDate { get; set; }

    public Guid ProjectId { get; set; }
    public ProjectEntity Project { get; set; }
    
    public ICollection<TaskEntity> Tasks { get; set; } = [];
}