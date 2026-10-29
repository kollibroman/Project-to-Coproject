namespace ProjectCowork.Api.Models;

public record ProjectRoleRequest
{
    public required string Name { get; init; }
    public required bool IsDefault { get; init; }
    public required IReadOnlyCollection<int> PermissionIds { get; init; }
}

public record ChangeMemberRoleRequest
{
    public required Guid RoleId { get; init; }
}
