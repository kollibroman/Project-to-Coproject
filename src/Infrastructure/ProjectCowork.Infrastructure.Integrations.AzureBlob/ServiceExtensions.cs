using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectCowork.Infrastructure.Configuration.Options;
using ProjectCowork.Infrastructure.Configuration.Utils;
using ProjectCowork.Infrastructure.Integrations.AzureBlob.Abstractions;
using ProjectCowork.Infrastructure.Integrations.AzureBlob.Internal;
using ProjectCowork.Infrastructure.Integrations.AzureBlob.Internal.Options;

namespace ProjectCowork.Infrastructure.Integrations.AzureBlob;

public static class ServiceExtensions
{
    public static IServiceCollection AddAzureBlob(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptionsWithRequiredFieldsValidation<BlobStorageSettings>();
        
        services.AddAzureClients(builder =>
        {
            builder.AddBlobServiceClient(
                configuration.GetRequiredSection(IntegrationSectionNameBuilder.BuildSectionName("BlobStorage:ConnectionString")));
        });
        
        services.AddScoped<IBlobService, BlobService>();
        
        return services;
    }
}