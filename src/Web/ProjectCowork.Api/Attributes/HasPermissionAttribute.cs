using Microsoft.AspNetCore.Authorization;
using ProjectCowork.Domain.Enums.Permissions;

namespace ProjectCowork.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class HasPermissionAttribute : AuthorizeAttribute
{
    public ResourceType Resource { get; }
    public ActionType Action { get; }
    
    public HasPermissionAttribute(ResourceType resource, ActionType action)
    {
        Resource = resource;
        Action = action;
        Policy = BuildPolicyName(resource, action);
    }
    private static string BuildPolicyName(ResourceType resource, ActionType action)
    {
        return $"Permission.{resource}.{action}";
    }
}