using Bogus;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Models.Project;

namespace ProjectCowork.Persistence.SeedData;

internal static class ProjectSeeder
{
    public static async Task SeedAsync(ProjectCoworkDbContext dbContext, SeedDataContext context, CancellationToken cancellationToken)
    {
        var faker = new Faker("pl");
        var projects = Enumerable.Range(0, 3).Select(index => new ProjectEntity
        {
            Name = faker.Company.CompanyName(),
            Description = faker.Lorem.Paragraph(),
            OwnerId = context.Users[index].Id,
            LastTaskFriendlyId = 0,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-index * 12),
            UpdatedAt = DateTimeOffset.UtcNow
        }).ToList();
        dbContext.Projects.AddRange(projects);
        await dbContext.SaveChangesAsync(cancellationToken);

        var roles = projects.SelectMany(project => CreateRoles(project, context.PermissionCatalog)).ToList();
        dbContext.ProjectRoles.AddRange(roles);
        await dbContext.SaveChangesAsync(cancellationToken);

        var members = projects.SelectMany((project, projectIndex) =>
        {
            var defaultRole = roles.Single(role => role.ProjectId == project.Id && role.IsDefault);
            return Enumerable.Range(1, 3).Select(memberIndex =>
            {
                var user = context.Users[(projectIndex + memberIndex) % context.Users.Count];
                var member = new ProjectMemberEntity
                {
                    UserId = user.Id,
                    User = user,
                    ProjectId = project.Id,
                    Project = project,
                    ProjectRoleId = defaultRole.Id,
                    ProjectRole = defaultRole
                };
                project.ProjectMembers.Add(member);
                return member;
            });
        }).ToList();
        dbContext.ProjectMembers.AddRange(members);
        await dbContext.SaveChangesAsync(cancellationToken);

        context.Projects.AddRange(projects);
        context.ProjectRoles.AddRange(roles);
    }

    private static IEnumerable<ProjectRoleEntity> CreateRoles(
        ProjectEntity project,
        IReadOnlyList<Domain.Models.Permissions.FeatureActionEntity> permissions)
    {
        foreach (var (name, isDefault) in new[] { ("Administrator", false), ("Członek", true), ("Obserwator", false) })
        {
            var role = new ProjectRoleEntity
            {
                Name = name,
                IsDefault = isDefault,
                ProjectId = project.Id,
                Project = project
            };
            role.FeatureActions = name switch
            {
                "Administrator" => permissions.ToList(),
                "Członek" => permissions.Where(permission =>
                    permission.Action.Name == nameof(ActionType.View)
                    || permission.Feature.Name == nameof(ResourceType.Management)
                    || (permission.Feature.Name == nameof(ResourceType.Repositories)
                        && permission.Action.Name == nameof(ActionType.Create))).ToList(),
                _ => permissions.Where(permission => permission.Action.Name == nameof(ActionType.View)).ToList()
            };
            yield return role;
        }
    }
}
