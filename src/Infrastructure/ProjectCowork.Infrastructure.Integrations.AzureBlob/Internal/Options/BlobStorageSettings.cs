using System.ComponentModel.DataAnnotations;
using ProjectCowork.Infrastructure.Abstractions.Options;
using ProjectCowork.Infrastructure.Configuration.Utils;

namespace ProjectCowork.Infrastructure.Integrations.AzureBlob.Internal.Options;

internal sealed record BlobStorageSettings : IProjectCoworkOptions
{

    public static string SectionName => IntegrationSectionNameBuilder.BuildSectionName("BlobStorage");
    
    [Required]
    public required string ConnectionString { get; init; }
}