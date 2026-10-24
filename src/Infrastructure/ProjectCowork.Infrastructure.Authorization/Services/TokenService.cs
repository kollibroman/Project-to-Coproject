using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProjectCowork.Domain.Models.Users;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Authorization.Settings;
using ProjectCowork.Persistence;

namespace ProjectCowork.Infrastructure.Authorization.Services;

internal sealed class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ProjectCoworkDbContext _dbContext;

    public TokenService(IOptions<JwtSettings> jwtSettings, ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
        _jwtSettings = jwtSettings.Value;
    }

    public Task<string> GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes );

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
    
    public Task<(string, DateTime)> GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        
        return Task.FromResult((Convert.ToBase64String(randomNumber), DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiresInDays)));
    }

    public async Task SaveRefreshTokenAsync(string refreshToken, Guid userId, DateTime expirationDate)
    {
        var existingToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (existingToken is not null)
        {
            _dbContext.RefreshTokens.Remove(existingToken);
        }
        
        var refreshTokenEntity = new RefreshTokenEntity
        {
            Token = RefreshTokenHasher.Hash(refreshToken),
            ExpiresAt = expirationDate,
            UserId = userId
        };
        
        _dbContext.Add(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var hash = RefreshTokenHasher.Hash(refreshToken);

        var existingToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == hash);

        if (existingToken is null)
        {
            return;
        }

        _dbContext.RefreshTokens.Remove(existingToken);
        await _dbContext.SaveChangesAsync();
    }
}