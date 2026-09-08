using ProjectCowork.Infrastructure.Integrations.AzureBlob.Models;

namespace ProjectCowork.Infrastructure.Integrations.AzureBlob.Abstractions;

public interface IBlobService
{
    Task UploadFileASync(SaveFileRequest request, CancellationToken ct);
    Task<Stream> DownloadFileASync(string fileName, CancellationToken ct);
}