using System.Security.Claims;

namespace ProjectCowork.Infrastructure.Authorization.Abstractions;

public interface ITokenService
{
    Task<string> GenerateAccessToken(IEnumerable<Claim> claims);
    Task<(string, DateTime)> GenerateRefreshToken();
    Task SaveRefreshTokenAsync(string refreshToken, Guid userId, DateTime expirationDate);
    Task RevokeRefreshTokenAsync(string refreshToken);
}
