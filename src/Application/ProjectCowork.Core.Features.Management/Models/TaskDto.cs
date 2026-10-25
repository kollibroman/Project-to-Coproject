using ProjectCowork.Domain.Enums.Management;
using TaskStatus = ProjectCowork.Domain.Enums.Management.TaskStatus;

namespace ProjectCowork.Core.Features.Management.Models;

public record TaskDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required TaskStatus Status { get; init; }
    public required TaskPriority Priority { get; init; }
    public DateOnly? FinishedDate { get; init; }
    public Guid? SprintId { get; init; }
    public Guid? AssignedUserId { get; init; }
}
