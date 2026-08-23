namespace ProjectCowork.Domain.Models.Permissions;

public class FeatureEntity
{
    public int Id { get; }
    public string Name { get; set; }

    public ICollection<ApplicationModuleEntity> Modules { get; set; } = [];
}