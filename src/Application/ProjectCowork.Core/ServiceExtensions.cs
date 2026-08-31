using System.Reflection;
using DispatchR.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectCowork.Core;

public static class ServiceExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        var featureAssemblies = configuration.GetSection("DispatchR:FeatureAssemblies").Get<string[]>() ?? [];
        var infraAssemblies = configuration.GetSection("DispatchR:InfrastructureAssemblies").Get<string[]>() ?? [];
        
        services.AddDispatchR(options =>
        {
            foreach (var assemblyName in featureAssemblies)
            {
                options.Assemblies.Add(Assembly.Load(assemblyName));
            }

            foreach (var assemblyName in infraAssemblies)
            {
                options.Assemblies.Add(Assembly.Load(assemblyName));
            }
        });
    
        return services;
    }
}