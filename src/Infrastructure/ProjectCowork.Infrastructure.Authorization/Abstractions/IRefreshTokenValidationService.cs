
namespace ProjectCowork.Infrastructure.Authorization.Abstractions;

public interface IRefreshTokenValidationService
{
    Task<Guid?> ValidateTokenAsync(string refreshToken);
}
