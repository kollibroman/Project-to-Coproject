using ProjectCowork.Domain.Models.Permissions;

namespace ProjectCowork.Domain.Models.Project;

public class ProjectRoleEntity
{
    public Guid Id { get; }

    public required string Name { get; set; }
    public required bool IsDefault { get; set; }
    
    public required Guid ProjectId { get; set; }
    public ProjectEntity Project { get; set; }

    public ICollection<FeatureActionEntity> FeatureActions { get; set; } = [];
}