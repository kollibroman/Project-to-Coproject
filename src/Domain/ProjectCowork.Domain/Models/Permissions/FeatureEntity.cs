namespace ProjectCowork.Domain.Models.Permissions;

public class FeatureEntity
{
    public int Id { get; }

    public ICollection<ApplicationModuleEntity> Modules { get; set; } = [];
}