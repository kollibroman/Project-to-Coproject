using System.Reflection;
using DispatchR.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectCowork.Core;

public static class ServiceExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDispatchR(options =>
        {
            options.Assemblies.Add(Assembly.Load(configuration["DispatchR:CoreAssembly"]));
        });
        
        return services;
    }
}