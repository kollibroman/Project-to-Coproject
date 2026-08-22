using Microsoft.AspNetCore.Authorization;
using ProjectCowork.Domain.Enums.Permissions;

namespace ProjectCowork.Infrastructure.Authorization.Models;

public class PermissionRequirement : IAuthorizationRequirement
{
    public ResourceType Resource { get; }
    public ActionType Action { get; }
    
    public PermissionRequirement(ResourceType resource, ActionType action)
    {
        Resource = resource;
        Action = action;
    }
}