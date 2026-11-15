using System.ComponentModel.DataAnnotations;
using ProjectCowork.Infrastructure.Abstractions.Options;
using ProjectCowork.Infrastructure.Configuration.Utils;

namespace ProjectCowork.Infrastructure.Integrations.Email.Internal.Options;

internal record EmailOptions : IProjectCoworkOptions
{
    public static string SectionName => IntegrationSectionNameBuilder.BuildSectionName("Email");

    [Required]
    public required string Username { get; init; }
    
    [Required]
    public required string Password { get; init; }
    
    [Required] 
    public required string Host { get; init; }
    
    [Required]
    public required int Port { get; init; }
}