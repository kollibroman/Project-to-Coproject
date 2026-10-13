using Microsoft.AspNetCore.Identity;
using ProjectCowork.Domain.Models.Posting;
using ProjectCowork.Domain.Models.Project;

namespace ProjectCowork.Domain.Models.Users;

public class UserEntity : IdentityUser<Guid>
{
    public ICollection<ProjectEntity> OwnedProjects { get; set; } = [];
    public ICollection<ProjectEntity> AssignedProjects { get; set; } = [];
    public ICollection<ProjectApplicationEntity> ProjectApplications { get; set; } = [];
}