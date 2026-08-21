using ProjectCowork.Domain.Models.Users;

namespace ProjectCowork.Domain.Models.Permissions;

public class RolePermissionEntity
{
    public int Id { get; set; }
    public Guid RoleId { get; set; }
    public RoleEntity Role { get; set; }
    
    public int FeatureActionId { get; set; }
    public FeatureActionEntity FeatureAction { get; set; } = null!;
}