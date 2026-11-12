using Azure.Storage.Blobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectCowork.Infrastructure.Integrations.AzureBlob.Internal.Options;
namespace ProjectCowork.Infrastructure.Integrations.AzureBlob.Internal;

internal sealed class BlobContainerInitializer : IHostedService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobStorageSettings _settings;
    private readonly ILogger<BlobContainerInitializer> _logger;

    public BlobContainerInitializer(BlobServiceClient blobServiceClient, IOptions<BlobStorageSettings> settings,
        ILogger<BlobContainerInitializer> logger)
    {
        _blobServiceClient = blobServiceClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var container = _blobServiceClient.GetBlobContainerClient(_settings.ContainerName);
            var response = await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            if (response is not null)
            {
                _logger.LogInformation("Created blob container {ContainerName}", _settings.ContainerName);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create blob container {ContainerName}", _settings.ContainerName);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
