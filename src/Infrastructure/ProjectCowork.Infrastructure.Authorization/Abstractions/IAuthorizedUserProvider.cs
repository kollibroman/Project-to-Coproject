using System.Security.Claims;

namespace ProjectCowork.Infrastructure.Authorization.Abstractions;

public interface IAuthorizedUserProvider
{
    Guid GetCurrentUserId();
    IEnumerable<Claim>? GetCurrentUserClaims();
}