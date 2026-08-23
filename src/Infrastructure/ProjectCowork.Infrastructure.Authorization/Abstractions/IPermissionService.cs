using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Infrastructure.Authorization.Models;

namespace ProjectCowork.Infrastructure.Authorization.Abstractions;

public interface IPermissionService
{
    Task<bool> UserHasPermissionAsync(Guid userId, ResourceType resource, ActionType action);
    Task<List<PermissionDto>> GetUserPermissionsAsync(Guid userId);
    Task<bool> RoleHasPermissionAsync(Guid roleId, ResourceType resource, ActionType action);
    Task AssignPermissionsToRoleAsync(Guid roleId, List<PermissionAssignmentDto> assignments);
    Task<List<PermissionStructureDto>> GetPermissionStructureAsync(int moduleId);
}