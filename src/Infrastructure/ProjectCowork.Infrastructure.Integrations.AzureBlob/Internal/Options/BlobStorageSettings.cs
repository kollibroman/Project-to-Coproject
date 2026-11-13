using System.ComponentModel.DataAnnotations;
using ProjectCowork.Infrastructure.Abstractions.Options;
using ProjectCowork.Infrastructure.Configuration.Utils;
namespace ProjectCowork.Infrastructure.Integrations.AzureBlob.Internal.Options;
internal sealed record BlobStorageSettings : IProjectCoworkOptions
{
    public static string SectionName => IntegrationSectionNameBuilder.BuildSectionName("BlobStorage");
    [Required]
    public required string ConnectionString { get; init; }

    // Nazwa kontenera w Azure: 3-63 znaki, małe litery, cyfry i '-'.
    [Required]
    [RegularExpression("^[a-z0-9](?!.*--)[a-z0-9-]{1,61}[a-z0-9]$")]
    public required string ContainerName { get; init; }
}
