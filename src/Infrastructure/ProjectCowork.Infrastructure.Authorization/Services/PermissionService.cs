using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Authorization.Models;
using ProjectCowork.Persistence;

namespace ProjectCowork.Infrastructure.Authorization.Services;

internal class PermissionService : IPermissionService
{
    private readonly ProjectCoworkDbContext _context;
    private readonly IMemoryCache _cache;
    
    public PermissionService(ProjectCoworkDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }
    public async Task<bool> UserHasPermissionAsync(Guid userId, ResourceType resource, ActionType action)
    {
        var cacheKey = $"permissions_{userId}";
        if (_cache.TryGetValue(cacheKey, out List<PermissionDto>? permissions))
        {
            return permissions.Any(p =>
                p.Resource == resource &&
                p.Action == action);    
        }
        
        var userRoles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var rolePermissions = await _context.RolePermissions
            .Where(rp => userRoles.Contains(rp.RoleId))
            .Include(rp => rp.FeatureAction)
            .ThenInclude(fa => fa.Feature)
            .Include(rp => rp.FeatureAction)
            .ThenInclude(fa => fa.Action)
            .ToListAsync();

        permissions =
        [
            .. rolePermissions.Select(rp => new PermissionDto
            {
                Resource = Enum.Parse<ResourceType>(rp.FeatureAction.Feature.Name),
                Action = Enum.Parse<ActionType>(rp.FeatureAction.Action.Name)
            })
        ];
            
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
            
        _cache.Set(cacheKey, permissions, cacheEntryOptions);
        
        return permissions.Any(p => 
            p.Resource == resource && 
            p.Action == action);
    }
    public async Task<List<PermissionDto>> GetUserPermissionsAsync(Guid userId)
    {
        var cacheKey = $"permissions_{userId}";
        if (_cache.TryGetValue(cacheKey, out List<PermissionDto>? permissions))
        {
            return permissions;
        }
        
        var userRoles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var rolePermissions = await _context.RolePermissions
            .Where(rp => userRoles.Contains(rp.RoleId))
            .Include(rp => rp.FeatureAction)
            .ThenInclude(fa => fa.Feature)
            .Include(rp => rp.FeatureAction)
            .ThenInclude(fa => fa.Action)
            .ToListAsync();
        
        permissions =
        [
            .. rolePermissions.Select(rp => new PermissionDto
            {
                Resource = Enum.Parse<ResourceType>(rp.FeatureAction.Feature.Name),
                Action = Enum.Parse<ActionType>(rp.FeatureAction.Action.Name)
            })
        ];
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
            
        _cache.Set(cacheKey, permissions, cacheEntryOptions);
        
        return permissions;
    }

    public Task<bool> RoleHasPermissionAsync(Guid roleId, ResourceType resource, ActionType action)
    {
        throw new NotImplementedException();
    }

    public Task AssignPermissionsToRoleAsync(Guid roleId, List<PermissionAssignmentDto> assignments)
    {
        throw new NotImplementedException();
    }

    public Task<List<PermissionStructureDto>> GetPermissionStructureAsync(int moduleId)
    {
        throw new NotImplementedException();
    }
}