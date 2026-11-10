using System.Text.Json.Serialization;

namespace ProjectCowork.Infrastructure.Integrations.Github.Models;

public record RepositoryModel
{
    [JsonPropertyName("full_name")]
    public required string FullName { get; init; }

    [JsonPropertyName("clone_url")]
    public required string CloneUrl { get; init; }

    [JsonPropertyName("owner")]
    public required OwnerDto Owner { get; init; }
}

public record OwnerDto
{
    [JsonPropertyName("login")]
    public required string Login { get; init; }

    [JsonPropertyName("url")]
    public required string Url { get; init; }
}