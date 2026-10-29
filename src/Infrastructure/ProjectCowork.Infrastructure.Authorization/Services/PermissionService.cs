using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Exceptions.Auth;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Infrastructure.Authorization.Services;

internal class PermissionService : IPermissionService
{
    private readonly ProjectCoworkDbContext _context;
    private readonly IAuthorizedUserProvider _authorizedUserProvider;

    public PermissionService(ProjectCoworkDbContext context, IAuthorizedUserProvider authorizedUserProvider)
    {
        _context = context;
        _authorizedUserProvider = authorizedUserProvider;
    }

    public Task<bool> HasProjectPermissionAsync(Guid userId, Guid projectId, ResourceType resource, ActionType action,
        CancellationToken ct = default)
    {
        var resourceName = resource.ToString();
        var actionName = action.ToString();

        return _context.Projects
            .AsNoTracking()
            .Where(p => p.Id == projectId)
            .AnyAsync(p => p.OwnerId == userId
                           || p.ProjectMembers.Any(m => m.UserId == userId
                                                        && m.ProjectRole.FeatureActions.Any(fa =>
                                                            fa.Feature.Name == resourceName
                                                            && fa.Action.Name == actionName)), ct);
    }

    public async Task EnsureProjectPermissionAsync(Guid projectId, ResourceType resource, ActionType action,
        CancellationToken ct = default)
    {
        Guid userId;

        try
        {
            userId = _authorizedUserProvider.GetCurrentUserId();
        }
        catch (UnauthorizedAccessException)
        {
            throw new UnauthenticatedException("User is not authenticated.");
        }

        // Nieistniejący projekt też daje 403, żeby nie zdradzać, które id istnieją.
        if (!await HasProjectPermissionAsync(userId, projectId, resource, action, ct))
        {
            throw new ForbiddenException($"Missing permission {resource}.{action} in project {projectId}.");
        }
    }
}
