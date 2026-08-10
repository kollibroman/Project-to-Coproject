namespace ProjectCowork.Domain.Models;

public class BaseTrackedEntity
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}