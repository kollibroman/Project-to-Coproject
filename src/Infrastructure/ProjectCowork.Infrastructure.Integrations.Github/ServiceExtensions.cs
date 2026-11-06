using Microsoft.Extensions.DependencyInjection;
using ProjectCowork.Infrastructure.Configuration.Options;
using ProjectCowork.Infrastructure.Integrations.Github.Abstractions;
using ProjectCowork.Infrastructure.Integrations.Github.Internal;
using ProjectCowork.Infrastructure.Integrations.Github.Internal.Options;
using RestSharp.Extensions.DependencyInjection;

namespace ProjectCowork.Infrastructure.Integrations.Github;

public static class ServiceExtensions
{
    public static IServiceCollection AddGithubServices(this IServiceCollection services)
    {
        services.AddRestClient();
        services.AddOptionsWithRequiredFieldsValidation<GithubOptions>();
        services.AddScoped<IGithubService, GithubService>();
        
        return services;
    }
}