using Microsoft.EntityFrameworkCore;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Infrastructure.Authorization.Services;

public class RefreshTokenValidationService : IRefreshTokenValidationService
{
    private readonly ProjectCoworkDbContext _dbContext;

    public RefreshTokenValidationService(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid?> ValidateTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var hash = RefreshTokenHasher.Hash(refreshToken);

        var tokenEntity = await _dbContext.RefreshTokens
            .Where(x => x.Token == hash)
            .Select(x => new
            {
                x.UserId,
                x.ExpiresAt
            })
            .FirstOrDefaultAsync();

        if (tokenEntity is null || DateTime.UtcNow >= tokenEntity.ExpiresAt)
        {
            return null;
        }

        return tokenEntity.UserId;
    }
}
