using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectCowork.Persistence.Interceptors;

namespace ProjectCowork.Persistence;

public static class ServiceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddSingleton<BasedTrackedEntityInterceptor>();
        
        services.AddDbContext<ProjectCoworkDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetRequiredService<BasedTrackedEntityInterceptor>());
            options.UseNpgsql(connectionString);
        });
        
        return services;
    }
}