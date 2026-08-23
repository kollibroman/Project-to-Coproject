using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
}