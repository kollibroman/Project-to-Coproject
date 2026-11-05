using System.ComponentModel.DataAnnotations;
using ProjectCowork.Infrastructure.Abstractions.Options;
using ProjectCowork.Infrastructure.Configuration.Utils;

namespace ProjectCowork.Infrastructure.Integrations.Github.Internal.Options;

internal class GithubOptions : IProjectCoworkOptions
{
    public static string SectionName => IntegrationSectionNameBuilder.BuildSectionName("Github");
    
    [Required]
    public required string BaseUrl { get; set; }
}