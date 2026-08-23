using System.Security.Claims;
using DispatchR.Abstractions.Send;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.User.Models;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Models.Users;
using ProjectCowork.Infrastructure.Authorization.Abstractions;

namespace ProjectCowork.Core.Features.User.Commands;

public record RegisterCommand : IRequest<RegisterCommand, Task<TokenDto>>
{
    public required string Email { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required RoleNames UserRole { get; init; }
}

internal sealed class RegisterCommandHandler : TokenCommandHandlerBase, IRequestHandler<RegisterCommand, Task<TokenDto>>
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly RoleManager<RoleEntity> _roleManager;

    public RegisterCommandHandler(UserManager<UserEntity> userManager, ITokenService tokenService, RoleManager<RoleEntity> roleManager) : base(tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<TokenDto> Handle(RegisterCommand request, CancellationToken ct)
    {
        var applicationUser = new UserEntity
        {
            Email = request.Email,
            UserName = request.Username,
        };

        await _userManager.CreateAsync(applicationUser, request.Password);

        var role = await _roleManager.Roles
            .Where(x => x.Name == request.UserRole.ToString())
            .FirstAsync(ct);
        
        await _userManager.AddToRoleAsync(applicationUser, role.Name);
        var currentRoles = await _userManager.GetRolesAsync(applicationUser);
        
        List<Claim> claims = 
        [
            new (ClaimTypes.NameIdentifier, applicationUser.Id.ToString()),
            new (ClaimTypes.Email, request.Email),
            ..currentRoles.Select(r => new Claim(ClaimTypes.Role, r))
        ];

        return await GenerateAndSaveTokensAsync(applicationUser.Id, claims);
    }
}