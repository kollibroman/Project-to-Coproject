using Microsoft.Extensions.DependencyInjection;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Authorization.Services;

namespace ProjectCowork.Infrastructure.Authorization;

public static class ServiceProvider
{
    public static IServiceCollection AddAuthorizationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizedUserProvider, AuthorizedUserProvider>();
        
        return services;
    }
}