using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectCowork.Infrastructure.Integrations.AzureBlob;

namespace ProjectCowork.Infrastructure.Integrations;

public static class ServiceExtensions
{
    public static IServiceCollection AddIntegrations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAzureBlob(configuration);
        
        return services;
    }
}