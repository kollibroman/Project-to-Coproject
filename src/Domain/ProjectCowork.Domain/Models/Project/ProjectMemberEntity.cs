using ProjectCowork.Domain.Models.Users;

namespace ProjectCowork.Domain.Models.Project;

public class ProjectMemberEntity
{
    public Guid Id { get; }

    public Guid UserId { get; set; }
    public UserEntity User { get; set; }
    
    public Guid ProjectId { get; set; }
    public ProjectEntity Project { get; set; }    
    
    public Guid ProjectRoleId { get; set; }
    public ProjectRoleEntity ProjectRole { get; set; }
}