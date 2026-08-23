using System.Security.Claims;

namespace ProjectCowork.Infrastructure.Authorization.Abstractions;

public interface ITokenService
{
    Task<string> GenerateAccessToken(IEnumerable<Claim> claims);
}