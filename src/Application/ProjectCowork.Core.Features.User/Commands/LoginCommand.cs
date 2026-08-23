using System.Net;
using System.Security.Claims;
using DispatchR.Abstractions.Send;
using Microsoft.AspNetCore.Identity;
using ProjectCowork.Core.Features.User.Models;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Domain.Models.Users;
using ProjectCowork.Infrastructure.Authorization.Abstractions;

namespace ProjectCowork.Core.Features.User.Commands;

public record LoginCommand : IRequest<LoginCommand, Task<TokenDto>>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

internal sealed class LoginCommandHandler : TokenCommandHandlerBase, IRequestHandler<LoginCommand, Task<TokenDto>>
{
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly UserManager<UserEntity> _userManager;

    public LoginCommandHandler(SignInManager<UserEntity> signInManager, ITokenService tokenService, UserManager<UserEntity> userManager) : base(tokenService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<TokenDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            throw new EntityNotFoundException("User not found", HttpStatusCode.NotFound);
        }
        
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid login attempt");
        }
        
        var currentRoles = await _userManager.GetRolesAsync(user);
        
        List<Claim> claims = 
        [
            new (ClaimTypes.NameIdentifier, user.Id.ToString()),
            new (ClaimTypes.Email, request.Email),
            ..currentRoles.Select(r => new Claim(ClaimTypes.Role, r))
        ];
        
        return await GenerateAndSaveTokensAsync(user.Id, claims);
    }
}