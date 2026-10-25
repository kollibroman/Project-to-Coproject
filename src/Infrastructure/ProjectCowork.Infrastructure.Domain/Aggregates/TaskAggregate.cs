using ProjectCowork.Domain.Enums.Management;
using ProjectCowork.Domain.Models.Management;
using ProjectCowork.Domain.Models.Project;
using TaskStatus = ProjectCowork.Domain.Enums.Management.TaskStatus;

namespace ProjectCowork.Infrastructure.Domain.Aggregates;

public class TaskAggregate
{
    private TaskEntity TaskItem { get; }

    public TaskAggregate(TaskEntity task)
    {
        ArgumentNullException.ThrowIfNull(task);
        TaskItem = task;
    }

    public static TaskEntity Create(ProjectEntity project, string name, string description, TaskPriority priority,
        TaskStatus status, SprintEntity? sprint, Guid? assignedUserId)
    {
        ValidateName(name);

        if (sprint is not null)
        {
            EnsureSprintBelongsToProject(sprint, project.Id);
        }
        
        var friendlyId = project.LastTaskFriendlyId != 0 
            ? project.LastTaskFriendlyId + 1 
            : project.LastTaskFriendlyId;
        
        return new TaskEntity
        {
            Name = name,
            Description = description,
            Status = status,
            Priority = priority,
            FinishedDate = status == TaskStatus.Done
                ? Today()
                : null,
            ProjectId = project.Id,
            SprintId = sprint?.Id,
            AssignedUserId = assignedUserId,
            FriendlyId = friendlyId,
        };
    }

    public void SetName(string name)
    {
        if (TaskItem.Name == name)
        {
            return;
        }

        ValidateName(name);
        TaskItem.Name = name;
    }

    public void SetDescription(string description)
    {
        if (TaskItem.Description == description)
        {
            return;
        }

        TaskItem.Description = description;
    }

    public void SetPriority(TaskPriority priority)
    {
        TaskItem.Priority = priority;
    }

    public void SetStatus(TaskStatus status)
    {
        if (TaskItem.Status == status)
        {
            return;
        }

        TaskItem.FinishedDate = status == TaskStatus.Done ? Today() : null;
        TaskItem.Status = status;
    }

    public void MoveToSprint(SprintEntity sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);

        if (TaskItem.SprintId == sprint.Id)
        {
            return;
        }

        EnsureSprintBelongsToProject(sprint, TaskItem.ProjectId);

        TaskItem.SprintId = sprint.Id;
    }

    public void MoveToBacklog()
    {
        TaskItem.SprintId = null;
        TaskItem.Sprint = null;
    }

    public void AssignUser(Guid userId)
    {
        TaskItem.AssignedUserId = userId;
    }

    public void Unassign()
    {
        TaskItem.AssignedUserId = null;
    }

    public void DeleteTask()
    {
        // TODO: Business logic
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Task name cannot be empty.");
        }
    }

    private static void EnsureSprintBelongsToProject(SprintEntity sprint, Guid projectId)
    {
        if (sprint.ProjectId != projectId)
        {
            throw new InvalidOperationException($"Sprint {sprint.Id} belongs to a different project.");
        }
    }

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.UtcNow);
}
