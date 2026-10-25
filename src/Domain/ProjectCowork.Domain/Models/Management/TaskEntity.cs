using ProjectCowork.Domain.Enums.Management;
using ProjectCowork.Domain.Models.Project;
using ProjectCowork.Domain.Models.Users;
using TaskStatus = ProjectCowork.Domain.Enums.Management.TaskStatus;

namespace ProjectCowork.Domain.Models.Management;

public class TaskEntity : BaseTrackedEntity
{
    public Guid Id { get; }

    public required string Name { get; set; }
    public required string Description { get; set; }

    public required TaskStatus Status { get; set; }
    public required TaskPriority Priority { get; set; }

    public DateOnly? FinishedDate { get; set; }

    public required long FriendlyId { get; init; }
    
    public required Guid? AssignedUserId { get; set; }
    public UserEntity? AssignedUser { get; set; }
    
    public Guid ProjectId { get; set; }
    public ProjectEntity Project { get; set; }
    
    public Guid? SprintId { get; set; }
    public SprintEntity? Sprint { get; set; }
}