using Microsoft.Extensions.DependencyInjection;
using ProjectCowork.Infrastructure.Integrations.Git.Abstractions;
using ProjectCowork.Infrastructure.Integrations.Git.Internal;

namespace ProjectCowork.Infrastructure.Integrations.Git;

public static class ServiceExtensions
{
    public static IServiceCollection AddGitServices(this IServiceCollection services)
    {
        services.AddScoped<IGitServerService, GitServerService>();
        services.AddScoped<IGitClientService, GitClientService>();
        
        return services;
    }
}