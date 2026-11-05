using ProjectCowork.Domain.Models.Users;

namespace ProjectCowork.Domain.Models.Project;

public class ProjectInvitationEntity : BaseTrackedEntity
{
    public Guid Id { get; }

    public required string InvitationContent { get; init; }

    public bool UserAccepted { get; set; }
    public bool IsActive { get; set; }

    public required Guid? RoleId { get; init; }
    
    public required Guid ProjectId { get; init; }
    public ProjectEntity Project { get; set; } = null!;
    
    public required Guid UserId { get; init; }
    public UserEntity User { get; set; } = null!;
}