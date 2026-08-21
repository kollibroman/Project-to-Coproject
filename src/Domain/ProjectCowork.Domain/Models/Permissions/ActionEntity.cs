namespace ProjectCowork.Domain.Models.Permissions;

public class ActionEntity
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<FeatureActionEntity> Features { get; set; } = [];
}