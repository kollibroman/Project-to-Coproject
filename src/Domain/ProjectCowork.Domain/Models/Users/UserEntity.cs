using Microsoft.AspNetCore.Identity;
using ProjectCowork.Domain.Models.Posting;
using ProjectCowork.Domain.Models.Project;

namespace ProjectCowork.Domain.Models.Users;

public class UserEntity : IdentityUser<Guid>
{
    public RefreshTokenEntity RefreshToken { get; set; }
    
    public ICollection<ProjectEntity> OwnedProjects { get; set; } = [];
    public ICollection<ProjectMemberEntity> AssignedProjects { get; set; } = [];
    public ICollection<ProjectApplicationEntity> ProjectApplications { get; set; } = [];
}