using System.Text.Json.Serialization;

namespace ProjectCowork.Core.Features.Repositories.Models;

[JsonConverter(typeof(JsonStringEnumConverter<TreeEntryType>))]
public enum TreeEntryType
{
    [JsonStringEnumMemberName("folder")]
    Folder,

    [JsonStringEnumMemberName("file")]
    File
}

public record TreeEntryDto
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required TreeEntryType Type { get; init; }
    public long? Size { get; init; }
}

public record FileContentDto
{
    public required string Path { get; init; }

    public string? Content { get; init; }

    public required bool IsBinary { get; init; }
    public required long Size { get; init; }
}

public record BranchDto
{
    public required string Name { get; init; }
    public required bool IsDefault { get; init; }
}

public record CommitDto
{
    public required string Sha { get; init; }
    public required string Message { get; init; }
    public required string AuthorName { get; init; }
    public string? AuthorEmail { get; init; }
    public required DateTimeOffset Date { get; init; }
}

public record CommitsPageDto
{
    public required IReadOnlyList<CommitDto> Items { get; init; }
    public required bool HasMore { get; init; }
}
