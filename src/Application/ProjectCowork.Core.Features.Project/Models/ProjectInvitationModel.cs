namespace ProjectCowork.Core.Features.Models;

public record ProjectInvitationModel
{
    public required Guid Id { get; init; }
    public required Guid ProjectId { get; init; }
    public required string ProjectName { get; init; }
    public required string ProjectDescription { get; init; }
    public required string InvitedBy { get; init; }
    public required string InvitationContent { get; init; }

    /// <summary>Rola, którą użytkownik dostanie po akceptacji. Null oznacza rolę domyślną projektu.</summary>
    public string? RoleName { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}
