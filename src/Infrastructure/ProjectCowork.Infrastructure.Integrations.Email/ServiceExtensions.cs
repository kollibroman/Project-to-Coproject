using Microsoft.Extensions.DependencyInjection;
using ProjectCowork.Infrastructure.Configuration.Options;
using ProjectCowork.Infrastructure.Integrations.Email.Abstractions;
using ProjectCowork.Infrastructure.Integrations.Email.Internal;
using ProjectCowork.Infrastructure.Integrations.Email.Internal.Options;

namespace ProjectCowork.Infrastructure.Integrations.Email;

public static class ServiceExtensions
{
    public static IServiceCollection AddEmailIntegration(this IServiceCollection services)
    {
        services.AddOptionsWithRequiredFieldsValidation<EmailOptions>();

        services.AddTransient<IMailService, MailService>();
        
        return services;
    }
}