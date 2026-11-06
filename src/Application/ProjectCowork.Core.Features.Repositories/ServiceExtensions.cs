using Microsoft.Extensions.DependencyInjection;
using ProjectCowork.Core.Features.Repositories.Abstractions;
using ProjectCowork.Core.Features.Repositories.Services;

namespace ProjectCowork.Core.Features.Repositories;

public static class ServiceExtensions
{
    public static IServiceCollection AddRepositoriesFeature(this IServiceCollection services)
    {
        services.AddScoped<IRepositoryAccessService, RepositoryAccessService>();

        return services;
    }
}
