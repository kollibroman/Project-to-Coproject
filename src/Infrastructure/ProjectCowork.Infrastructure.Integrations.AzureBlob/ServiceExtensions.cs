using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

        var conectionString =
            configuration.GetRequiredSection(
                IntegrationSectionNameBuilder.BuildSectionName("BlobStorage:ConnectionString"));
        
        services.AddAzureClients(builder =>
        {
            builder.AddBlobServiceClient(conectionString);
        });

        services.AddScoped<BlobContainerClient>(sp =>
        {
            var blobServiceClient = sp.GetRequiredService<BlobServiceClient>();
            var settings = sp.GetRequiredService<IOptions<BlobStorageSettings>>().Value;
            
            return blobServiceClient.GetBlobContainerClient("test");
        });
        
        services.AddScoped<IBlobService, BlobService>();
        
        return services;
    }
}