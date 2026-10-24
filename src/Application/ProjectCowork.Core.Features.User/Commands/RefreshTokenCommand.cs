using System.Net;
using System.Security.Claims;
using DispatchR.Abstractions.Send;
using Microsoft.AspNetCore.Identity;
using ProjectCowork.Core.Features.User.Models;
using ProjectCowork.Domain.Exceptions.Auth;
using ProjectCowork.Domain.Models.Users;
using ProjectCowork.Infrastructure.Authorization.Abstractions;

namespace ProjectCowork.Core.Features.User.Commands;

public record RefreshTokenCommand : IRequest<RefreshTokenCommand, Task<TokenDto>>
{
    public required string RefreshToken { get; init; }
}

internal sealed class RefreshTokenCommandHandler : TokenCommandHandlerBase, IRequestHandler<RefreshTokenCommand, Task<TokenDto>>
{
    private readonly IRefreshTokenValidationService _refreshTokenValidationService;
    private readonly UserManager<UserEntity> _userManager;
    
    public RefreshTokenCommandHandler(ITokenService tokenService, IRefreshTokenValidationService refreshTokenValidationService, UserManager<UserEntity> userManager) : base(tokenService)
    {
        _refreshTokenValidationService = refreshTokenValidationService;
        _userManager = userManager;
    }

    public async Task<TokenDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = await _refreshTokenValidationService.ValidateTokenAsync(request.RefreshToken);

        if (userId is null)
        {
            throw new InvalidTokenException("Invalid refresh token", HttpStatusCode.Unauthorized);
        }

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());

        if (user is null)
        {
            throw new InvalidTokenException("Invalid refresh token", HttpStatusCode.Unauthorized);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        List<Claim> claims =
        [
            new (ClaimTypes.NameIdentifier, user.Id.ToString()),
            new (ClaimTypes.Email, user.Email ?? string.Empty),
            ..currentRoles.Select(r => new Claim(ClaimTypes.Role, r))
        ];

        return await GenerateAndSaveTokensAsync(user.Id, claims);
    }
}
