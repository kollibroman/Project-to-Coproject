using Microsoft.Extensions.DependencyInjection;
using ProjectCowork.Infrastructure.Abstractions.Options;

namespace ProjectCowork.Infrastructure.Configuration.Options;

public static class OptionsExtensions
{
    public static IServiceCollection AddOptionsWithRequiredFieldsValidation<T>(this IServiceCollection services) where T : class, IProjectCoworkOptions
    {
        services.AddOptions<T>()
            .BindConfiguration(T.SectionName)
            .ValidateOnStart()
            .ValidateDataAnnotations();
        
        return services;
    }
}