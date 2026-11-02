using ProjectCowork.Domain.Models.Management;
using ProjectCowork.Domain.Models.Posting;
using ProjectCowork.Domain.Models.Repositories;
using ProjectCowork.Domain.Models.Users;

namespace ProjectCowork.Domain.Models.Project;

public class ProjectEntity : BaseTrackedEntity
{
    public Guid Id { get; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    
    public required Guid OwnerId { get; set; }

    public UserEntity Owner { get; set; } = null!;

    public required long LastTaskFriendlyId { get; set; } = 0;
    
    public ICollection<SprintEntity> Sprints { get; set; } = [];
    public ICollection<TaskEntity> Tasks { get; set; } = [];
    public ICollection<ProjectPostingEntity> ProjectPostings { get; set; } = [];
    public ICollection<ProjectMemberEntity> ProjectMembers { get; set; } = [];
    public ICollection<ProjectRoleEntity> ProjectRoles { get; set; } = [];
    public ICollection<RepositoryEntity> Repositories { get; set; } = [];
}