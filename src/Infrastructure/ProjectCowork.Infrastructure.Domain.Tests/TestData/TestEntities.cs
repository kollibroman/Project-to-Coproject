using ProjectCowork.Domain.Enums.Management;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Models.Management;
using ProjectCowork.Domain.Models.Permissions;
using ProjectCowork.Domain.Models.Project;
using ProjectCowork.Domain.Models.Users;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using TaskStatus = ProjectCowork.Domain.Enums.Management.TaskStatus;

namespace ProjectCowork.Infrastructure.Domain.Tests.TestData;

internal static class TestEntities
{
    /// <summary>Katalog uprawnień taki jak w PermissionSeeder: każdy ResourceType × każdy ActionType.</summary>
    public static List<FeatureActionEntity> PermissionCatalog()
    {
        var actions = Enum.GetValues<ActionType>()
            .Select((action, index) => new ActionEntity { Id = index + 1, Name = action.ToString() })
            .ToList();

        var catalog = new List<FeatureActionEntity>();
        var featureId = 1;

        foreach (var resource in Enum.GetValues<ResourceType>())
        {
            var feature = new FeatureEntity { Name = resource.ToString() }.WithId<FeatureEntity, int>(featureId++);

            foreach (var action in actions)
            {
                catalog.Add(new FeatureActionEntity
                {
                    Id = catalog.Count + 1,
                    FeatureId = feature.Id,
                    Feature = feature,
                    ActionId = action.Id,
                    Action = action
                });
            }
        }

        return catalog;
    }

    public static FeatureActionEntity Permission(this IEnumerable<FeatureActionEntity> catalog,
        ResourceType resource, ActionType action)
    {
        return catalog.Single(x => x.Feature.Name == resource.ToString() && x.Action.Name == action.ToString());
    }

    public static string Key(this FeatureActionEntity permission) =>
        $"{permission.Feature.Name}.{permission.Action.Name}";

    /// <summary>Projekt utworzony przez agregat, z nadanymi Id (w produkcji robi to EF Core).</summary>
    public static ProjectEntity Project(Guid? ownerId = null, IReadOnlyCollection<FeatureActionEntity>? catalog = null)
    {
        var project = ProjectAggregate.Create("Projekt testowy", "Opis", ownerId ?? Guid.NewGuid(),
            catalog ?? PermissionCatalog());

        project.WithNewId();

        foreach (var role in project.ProjectRoles)
        {
            role.WithNewId();
            role.ProjectId = project.Id;
        }

        return project;
    }

    public static ProjectRoleEntity Role(this ProjectEntity project, string name) =>
        project.ProjectRoles.Single(x => x.Name == name);

    public static UserEntity User() => new() { Id = Guid.NewGuid(), UserName = "user", Email = "user@example.com" };

    public static SprintEntity Sprint(Guid projectId) => new SprintEntity
    {
        Name = "Sprint 1",
        StartDate = new DateOnly(2026, 11, 1),
        EndDate = new DateOnly(2026, 11, 14),
        ProjectId = projectId
    }.WithNewId();

    public static TaskEntity Task(Guid projectId, TaskStatus status = TaskStatus.Todo, Guid? sprintId = null) =>
        new TaskEntity
        {
            Name = "Zadanie",
            Description = "Opis",
            Status = status,
            Priority = TaskPriority.Normal,
            FriendlyId = 1,
            AssignedUserId = null,
            ProjectId = projectId,
            SprintId = sprintId
        }.WithNewId();
}
