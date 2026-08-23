using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectCowork.Infrastructure.Authorization;
using ProjectCowork.Persistence;

namespace ProjectCowork.Infrastructure;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddAuthorizationServices(configuration);
        
        return services;
    }
}