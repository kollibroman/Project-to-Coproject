namespace ProjectCowork.Infrastructure.Integrations.AzureBlob.Models;

public record SaveFileRequest
{
    public required string ContainerName { get; init; }
    public required string FileName { get; init; }
    public required Stream Content { get; init; }
}