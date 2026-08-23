using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectCowork.Infrastructure.Abstractions.Options;

namespace ProjectCowork.Infrastructure.Configuration.Options;

public static class OptionsExtensions
{
    public static IServiceCollection AddOptionsWithRequiredFieldsValidation<T>(this IServiceCollection services, IConfiguration configuration) where T : class, IProjectCoworkOptions
    {
        services.AddOptions<T>()
            .Bind(configuration.GetSection(T.SectionName))
            .ValidateOnStart()
            .ValidateDataAnnotations();
        
        return services;
    }
}