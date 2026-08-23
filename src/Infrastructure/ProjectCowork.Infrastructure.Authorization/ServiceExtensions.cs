using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectCowork.Domain.Models.Users;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Authorization.Handlers;
using ProjectCowork.Infrastructure.Authorization.Services;
using ProjectCowork.Infrastructure.Authorization.Settings;
using ProjectCowork.Infrastructure.Configuration.Options;
using ProjectCowork.Persistence;

namespace ProjectCowork.Infrastructure.Authorization;

public static class ServiceExtensions
{
    public static IServiceCollection AddAuthorizationServices(this IServiceCollection services, IConfiguration configuration)
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
        
        services.AddOptionsWithRequiredFieldsValidation<JwtSettings>(configuration);
        
        services.AddScoped<IAuthorizedUserProvider, AuthorizedUserProvider>()
            .AddScoped<IPermissionService, PermissionService>()
            .AddScoped<ITokenService, TokenService>();
        
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        
        return services;
    }
}