using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ProjectCowork.Domain.Models.Users;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Authorization.Services;
using ProjectCowork.Infrastructure.Authorization.Settings;
using ProjectCowork.Infrastructure.Configuration.Options;
using ProjectCowork.Persistence;

namespace ProjectCowork.Infrastructure.Authorization;

public static class ServiceExtensions
{
    public static IServiceCollection AddAuthorizationServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        
        services.AddIdentityCore<UserEntity>(options => 
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddRoles<RoleEntity>() 
        .AddEntityFrameworkStores<ProjectCoworkDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();
        
        services.AddOptionsWithRequiredFieldsValidation<JwtSettings>();
        
        // Uprawnienia są sprawdzane per projekt w handlerach (IPermissionService.EnsureProjectPermissionAsync),
        // a nie przez globalne polityki ASP.NET.
        services.AddScoped<IAuthorizedUserProvider, AuthorizedUserProvider>()
            .AddScoped<IPermissionService, PermissionService>()
            .AddScoped<ITokenService, TokenService>()
            .AddScoped<IRefreshTokenValidationService, RefreshTokenValidationService>();
        
        return services;
    }
}
