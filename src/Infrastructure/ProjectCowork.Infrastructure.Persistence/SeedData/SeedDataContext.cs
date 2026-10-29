using ProjectCowork.Domain.Models.Management;
using ProjectCowork.Domain.Models.Posting;
using ProjectCowork.Domain.Models.Project;
using ProjectCowork.Domain.Models.Repositories;
using ProjectCowork.Domain.Models.Users;
using ProjectCowork.Domain.Models.Permissions;

namespace ProjectCowork.Persistence.SeedData;

internal sealed class SeedDataContext
{
    public required List<UserEntity> Users { get; init; }
    public required List<ProjectEntity> Projects { get; init; }
    public required List<ProjectRoleEntity> ProjectRoles { get; init; }
    public required List<SprintEntity> Sprints { get; init; }
    public required List<TaskEntity> Tasks { get; init; }
    public required List<RepositoryEntity> Repositories { get; init; }
    public required List<ProjectPostingEntity> Postings { get; init; }
    public required List<ProjectApplicationEntity> Applications { get; init; }
    public required List<FeatureActionEntity> PermissionCatalog { get; init; }
}
