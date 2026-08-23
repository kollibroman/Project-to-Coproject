using ProjectCowork.Domain.Enums.Permissions;

namespace ProjectCowork.Infrastructure.Authorization.Models;

public record PermissionDto
{
    public required ResourceType Resource { get; init; }
    public required ActionType Action { get; init; }
}
public record PermissionAssignmentDto
{
    public required ResourceType Resource { get; init; }
    public required ActionType Action { get; init; }
}
public record PermissionStructureDto
{
    public required int ModuleId { get; init; }
    public required string ModuleName { get; init; }
    public required List<FeaturePermissionDto> Features { get; init; } = [];
}
public record FeaturePermissionDto
{
    public required int FeatureId { get; init; }
    public required string FeatureName { get; init; }
    public required List<ActionPermissionDto> Actions { get; init; } = [];
}
public record ActionPermissionDto
{
    public required int ActionId { get; init; }
    public required string ActionName { get; init; }
    public required bool IsSelected { get; init; }
    public required bool IsDisabled { get; init; }
}