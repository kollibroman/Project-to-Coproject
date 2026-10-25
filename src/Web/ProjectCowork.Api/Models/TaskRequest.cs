using ProjectCowork.Domain.Enums.Management;
using TaskStatus = ProjectCowork.Domain.Enums.Management.TaskStatus;

namespace ProjectCowork.Api.Models;

public abstract record TaskRequestBase
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required TaskPriority Priority { get; init; }
    public Guid? SprintId { get; init; }
    public Guid? AssignedUserId { get; init; }
}

public record AddTaskRequest : TaskRequestBase
{
    public required Guid ProjectId { get; init; }
    public TaskStatus Status { get; init; } = TaskStatus.Todo;
}

public record EditTaskRequest : TaskRequestBase
{
    public required TaskStatus Status { get; init; }
}
