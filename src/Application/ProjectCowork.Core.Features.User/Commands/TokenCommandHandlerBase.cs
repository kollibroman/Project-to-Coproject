using System.Security.Claims;
using ProjectCowork.Core.Features.User.Models;
using ProjectCowork.Infrastructure.Authorization.Abstractions;

namespace ProjectCowork.Core.Features.User.Commands;

internal abstract class TokenCommandHandlerBase
{
    private readonly ITokenService _tokenService;

    protected TokenCommandHandlerBase(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    protected async Task<TokenDto> GenerateAndSaveTokensAsync(Guid userId, IEnumerable<Claim> claims)
    {
        // TODO: token refreshing
        var accessToken = await _tokenService.GenerateAccessToken(claims);
        
        return new TokenDto
        {
            AccessToken = accessToken
        };
    }
}