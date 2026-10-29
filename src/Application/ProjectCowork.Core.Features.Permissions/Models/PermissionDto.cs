using ProjectCowork.Domain.Enums.Permissions;

namespace ProjectCowork.Core.Features.Permissions.Models;

public record PermissionDto
{
    public required int Id { get; init; }
    public required ResourceType Resource { get; init; }
    public required ActionType Action { get; init; }
}
