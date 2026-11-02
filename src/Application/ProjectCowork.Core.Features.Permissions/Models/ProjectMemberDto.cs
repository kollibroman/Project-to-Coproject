namespace ProjectCowork.Core.Features.Permissions.Models;

public record ProjectMemberDto
{
    public required Guid UserId { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }

    /// <summary>Null dla właściciela, który nie ma roli i zawsze ma pełne uprawnienia.</summary>
    public required Guid? RoleId { get; init; }
    public required string RoleName { get; init; }
    public required bool IsOwner { get; init; }
}
