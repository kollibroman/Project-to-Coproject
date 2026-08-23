using System.ComponentModel.DataAnnotations;
using ProjectCowork.Infrastructure.Abstractions.Options;

namespace ProjectCowork.Infrastructure.Authorization.Settings;

public record JwtSettings : IProjectCoworkOptions
{
    public static string SectionName => "Jwt";
    
    [Required]
    public required string Key { get; init; }
    
    [Required]
    public required string Issuer { get; init; }
    
    [Required]
    public required string Audience { get; init; }
    
    [Required]
    public required int ExpiresInMinutes { get; init; }
}