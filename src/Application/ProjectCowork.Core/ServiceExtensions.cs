using System.Reflection;
using DispatchR.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectCowork.Core;

public static class ServiceExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        var assemblies = configuration.GetSection("DispatchR:FeatureAssemblies").Get<string[]>() ?? Array.Empty<string>();

        services.AddDispatchR(options =>
        {
            foreach (var assemblyName in assemblies)
            {
                options.Assemblies.Add(Assembly.Load(assemblyName));
            }
        });
    
        return services;
    }
}