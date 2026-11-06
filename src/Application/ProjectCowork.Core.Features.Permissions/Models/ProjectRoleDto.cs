namespace ProjectCowork.Core.Features.Permissions.Models;

public record ProjectRoleDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required bool IsDefault { get; init; }
    public required IReadOnlyCollection<int> PermissionIds { get; init; }
    public required int MemberCount { get; init; }
}
