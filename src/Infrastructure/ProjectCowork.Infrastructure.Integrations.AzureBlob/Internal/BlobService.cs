using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using ProjectCowork.Infrastructure.Integrations.AzureBlob.Abstractions;
using ProjectCowork.Infrastructure.Integrations.AzureBlob.Models;

namespace ProjectCowork.Infrastructure.Integrations.AzureBlob.Internal;

internal class BlobService : IBlobService
{
    private readonly BlobContainerClient  _blobContainerClient;
    private readonly ILogger<BlobService> _logger;

    public BlobService(BlobContainerClient blobContainerClient, ILogger<BlobService> logger)
    {
        _blobContainerClient = blobContainerClient;
        _logger = logger;
    }

    public async Task UploadFileASync(SaveFileRequest request, CancellationToken ct)
    {
        try
        {
            var client = _blobContainerClient.GetBlobClient($"{request.ContainerName}/{request.FileName}");
            await client.UploadAsync(request.Content, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    public async Task<Stream> DownloadFileASync(string fileName, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}