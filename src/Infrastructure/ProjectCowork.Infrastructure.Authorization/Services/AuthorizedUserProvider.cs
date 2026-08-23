using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ProjectCowork.Infrastructure.Authorization.Abstractions;

namespace ProjectCowork.Infrastructure.Authorization.Services;

internal sealed class AuthorizedUserProvider : IAuthorizedUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizedUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentUserId()
    { 
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return currentUserId is not null ? Guid.Parse(currentUserId) : throw new UnauthorizedAccessException();
    }

    public IEnumerable<Claim>? GetCurrentUserClaims()
    {
        return _httpContextAccessor.HttpContext?.User.Claims;
    }
}