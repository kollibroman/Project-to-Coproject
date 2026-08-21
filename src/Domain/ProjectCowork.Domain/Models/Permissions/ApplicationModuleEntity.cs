namespace ProjectCowork.Domain.Models.Permissions;

public class ApplicationModuleEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }

    public ICollection<FeatureEntity> Features { get; set; } = [];
}