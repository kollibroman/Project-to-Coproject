using System.Net;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Exceptions.Projects;
using ProjectCowork.Domain.Models.Permissions;
using ProjectCowork.Domain.Models.Project;
using ProjectCowork.Domain.Models.Users;

namespace ProjectCowork.Infrastructure.Domain.Aggregates;

public class ProjectAggregate
{
    private ProjectEntity Project { get; }

    public ProjectAggregate(ProjectEntity project)
    {
        ArgumentNullException.ThrowIfNull(project);
        Project = project;
    }

    private sealed record RoleTemplate(string Name, bool IsDefault, Func<ResourceType, ActionType, bool> Grants);

    private static readonly RoleTemplate[] DefaultRoles =
    [
        new("Administrator", false, (_, _) => true),
        new("Członek", true, (resource, action) =>
            action == ActionType.View
            || resource == ResourceType.Management
            || (resource == ResourceType.Repositories && action == ActionType.Create)),
        new("Obserwator", false, (_, action) => action == ActionType.View),
    ];

    public static ProjectEntity Create(string name, string description, Guid ownerId,
        IReadOnlyCollection<FeatureActionEntity> permissionCatalog)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty.");

        var project = new ProjectEntity
        {
            Name = name,
            Description = description,
            OwnerId = ownerId,
            LastTaskFriendlyId = 0
        };

        foreach (var template in DefaultRoles)
        {
            var role = new ProjectRoleEntity
            {
                Name = template.Name,
                IsDefault = template.IsDefault,
                ProjectId = project.Id,
                Project = project
            };

            foreach (var permission in permissionCatalog)
            {
                if (TryParse(permission, out var resource, out var action) && template.Grants(resource, action))
                {
                    role.FeatureActions.Add(permission);
                }
            }

            project.ProjectRoles.Add(role);
        }

        return project;
    }

    public void SetProjectName(string name)
    {
        if (Project.Name == name)
        {
            return;
        }
        
        Project.Name = name;
    }
    
    public void SetDescription(string description)
    {
        if (Project.Description == description)
        {
            return;
        }

        Project.Description = description;
    }

    public ProjectRoleEntity AddRole(string name, bool isDefault, IEnumerable<FeatureActionEntity> permissions)
    {
        var trimmedName = ValidateRoleName(name, null);

        var role = new ProjectRoleEntity
        {
            Name = trimmedName,
            IsDefault = false,
            ProjectId = Project.Id,
            Project = Project
        };

        SetRolePermissions(role, permissions);
        Project.ProjectRoles.Add(role);

        if (isDefault)
        {
            MakeDefault(role);
        }

        return role;
    }

    public void UpdateRole(Guid roleId, string name, bool isDefault, IEnumerable<FeatureActionEntity> permissions)
    {
        var role = GetRole(roleId);

        if (role.IsDefault && !isDefault)
        {
            throw new ProjectAccessException(
                "Project must have a default role. Mark another role as default instead.", HttpStatusCode.BadRequest);
        }

        role.Name = ValidateRoleName(name, roleId);
        SetRolePermissions(role, permissions);

        if (isDefault)
        {
            MakeDefault(role);
        }
    }

    public void RemoveRole(Guid roleId)
    {
        var role = GetRole(roleId);

        if (role.IsDefault)
        {
            throw new ProjectAccessException("Default role cannot be deleted.");
        }

        if (Project.ProjectMembers.Any(x => x.ProjectRoleId == roleId))
        {
            throw new ProjectAccessException("Role has members assigned. Change their role first.");
        }

        Project.ProjectRoles.Remove(role);
    }

    public ProjectMemberEntity AddMember(UserEntity user, Guid? roleId)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (Project.OwnerId == user.Id)
        {
            throw new ProjectAccessException("Owner is already part of the project.");
        }

        if (Project.ProjectMembers.Any(x => x.UserId == user.Id))
        {
            throw new ProjectAccessException("User is already a member of the project.");
        }

        var role = roleId is null ? GetDefaultRole() : GetRole(roleId.Value, HttpStatusCode.BadRequest);

        var member = new ProjectMemberEntity
        {
            UserId = user.Id,
            User = user,
            ProjectId = Project.Id,
            Project = Project,
            ProjectRoleId = role.Id,
            ProjectRole = role
        };

        Project.ProjectMembers.Add(member);

        return member;
    }

    public void ChangeMemberRole(Guid userId, Guid roleId)
    {
        EnsureNotOwner(userId, "Owner has no role and always has full permissions.");

        var member = GetMember(userId);
        var role = GetRole(roleId, HttpStatusCode.BadRequest);

        member.ProjectRoleId = role.Id;
        member.ProjectRole = role;
    }

    public void RemoveMember(Guid userId)
    {
        EnsureNotOwner(userId, "Owner cannot be removed from the project.");

        var member = GetMember(userId);

        Project.ProjectMembers.Remove(member);
    }

    public void DeleteProject()
    {
        // TODO: Business logic
    }

    private string ValidateRoleName(string name, Guid? roleId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ProjectAccessException("Role name cannot be empty.", HttpStatusCode.BadRequest);
        }

        var trimmed = name.Trim();

        var duplicate = Project.ProjectRoles.Any(x =>
            x.Id != roleId && string.Equals(x.Name, trimmed, StringComparison.OrdinalIgnoreCase));

        if (duplicate)
        {
            throw new ProjectAccessException($"Role '{trimmed}' already exists in this project.");
        }

        return trimmed;
    }

    private static void SetRolePermissions(ProjectRoleEntity role, IEnumerable<FeatureActionEntity> permissions)
    {
        var target = permissions.DistinctBy(x => x.Id).ToList();
        var targetIds = target.Select(x => x.Id).ToHashSet();

        foreach (var existing in role.FeatureActions.Where(x => !targetIds.Contains(x.Id)).ToList())
        {
            role.FeatureActions.Remove(existing);
        }

        foreach (var permission in target.Where(x => role.FeatureActions.All(fa => fa.Id != x.Id)))
        {
            role.FeatureActions.Add(permission);
        }
    }

    private void MakeDefault(ProjectRoleEntity role)
    {
        foreach (var other in Project.ProjectRoles)
        {
            other.IsDefault = ReferenceEquals(other, role);
        }
    }

    private ProjectRoleEntity GetRole(Guid roleId, HttpStatusCode notFoundStatus = HttpStatusCode.NotFound)
    {
        return Project.ProjectRoles.FirstOrDefault(x => x.Id == roleId)
               ?? throw new ProjectAccessException($"Role {roleId} does not exist in this project.", notFoundStatus);
    }

    private ProjectRoleEntity GetDefaultRole()
    {
        return Project.ProjectRoles.FirstOrDefault(x => x.IsDefault)
               ?? throw new ProjectAccessException("Project has no default role.", HttpStatusCode.InternalServerError);
    }

    private ProjectMemberEntity GetMember(Guid userId)
    {
        return Project.ProjectMembers.FirstOrDefault(x => x.UserId == userId)
               ?? throw new ProjectAccessException($"User {userId} is not a member of this project.", HttpStatusCode.NotFound);
    }

    private void EnsureNotOwner(Guid userId, string message)
    {
        if (Project.OwnerId == userId)
        {
            throw new ProjectAccessException(message, HttpStatusCode.BadRequest);
        }
    }

    private static bool TryParse(FeatureActionEntity permission, out ResourceType resource, out ActionType action)
    {
        action = default;
        return Enum.TryParse(permission.Feature?.Name, out resource)
               && Enum.TryParse(permission.Action?.Name, out action);
    }
}
