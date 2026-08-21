namespace ProjectCowork.Domain.Models.Permissions;

public class FeatureActionEntity
{
    public int Id { get; set; }
    public int FeatureId { get; set; }
    public FeatureEntity Feature { get; set; }
    public int ActionId { get; set; }
    public ActionEntity Action { get; set; }

    public ICollection<RolePermissionEntity> RolePermissions { get; set; } = [];
}