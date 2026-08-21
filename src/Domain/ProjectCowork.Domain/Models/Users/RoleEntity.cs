using Microsoft.AspNetCore.Identity;
using ProjectCowork.Domain.Models.Permissions;

namespace ProjectCowork.Domain.Models.Users;

public class RoleEntity : IdentityRole<Guid>
{
    public ICollection<RolePermissionEntity> Permissions { get; set; } = [];
}