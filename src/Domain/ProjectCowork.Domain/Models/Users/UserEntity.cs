using Microsoft.AspNetCore.Identity;
using ProjectCowork.Domain.Models.Project;

namespace ProjectCowork.Domain.Models.Users;

public class UserEntity : IdentityUser<Guid>
{
    public Guid RoleId { get; set; }
    public RoleEntity Role { get; set; }
    
    public ICollection<ProjectEntity> OwnedProjects { get; set; } = null!;
    public ICollection<ProjectEntity> AssignedProjects { get; set; } = null!;
}