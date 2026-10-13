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
        var roleName = request.UserRole.ToString();

        if (!await _roleManager.RoleExistsAsync(roleName) && (request.UserRole != RoleNames.Admin &&  request.UserRole != RoleNames.User && request.UserRole != RoleNames.Business))
        {
            throw new InvalidOperationException($"Role '{roleName}' does not exist.");
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new RoleEntity
            {
                Name = roleName
            });
        }
        
        var applicationUser = new UserEntity
        {
            Email = request.Email,
            UserName = request.Username,
        };

        var createResult = await _userManager.CreateAsync(applicationUser, request.Password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"User creation failed: {string.Join("; ", createResult.Errors.Select(e => e.Description))}");
        }

        var roleResult = await _userManager.AddToRoleAsync(applicationUser, roleName);
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Assigning role failed: {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");
        }
        
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