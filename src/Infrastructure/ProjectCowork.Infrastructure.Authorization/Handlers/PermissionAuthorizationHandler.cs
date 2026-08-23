using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Authorization.Models;

namespace ProjectCowork.Infrastructure.Authorization.Handlers;

internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public PermissionAuthorizationHandler(IHttpContextAccessor httpContextAccessor, IServiceScopeFactory serviceScopeFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceScopeFactory = serviceScopeFactory;
    }
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PermissionRequirement requirement)
    {
        if (context.User == null)
        {
            context.Fail();
            return;
        }
        
        var userIdentity = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdentity))
        {
            context.Fail();
            return;
        }
        
        var userId = Guid.Parse(userIdentity);

        using var scoper = _serviceScopeFactory.CreateScope();
        var permissionService = scoper.ServiceProvider.GetRequiredService<IPermissionService>();
        
        var hasPermission = await permissionService
            .UserHasPermissionAsync(userId, requirement.Resource, requirement.Action);
        
        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}