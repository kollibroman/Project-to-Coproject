using ProjectCowork.Domain.Models.Users;

namespace ProjectCowork.Domain.Models.Project;

public class ProjectEntity : BaseTrackedEntity
{
    public Guid Id { get; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    
    public required Guid OwnerId { get; set; }
    public UserEntity Owner { get; set; }

    public ICollection<UserEntity> Collaborators { get; set; } = [];
}