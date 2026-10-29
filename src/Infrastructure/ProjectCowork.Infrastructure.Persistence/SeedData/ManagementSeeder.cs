using Bogus;
using ProjectCowork.Domain.Enums.Management;
using ProjectCowork.Domain.Models.Management;
using TaskStatus = ProjectCowork.Domain.Enums.Management.TaskStatus;

namespace ProjectCowork.Persistence.SeedData;

internal static class ManagementSeeder
{
    public static async Task SeedAsync(ProjectCoworkDbContext dbContext, SeedDataContext context, CancellationToken cancellationToken)
    {
        var faker = new Faker("pl");
        var sprints = context.Projects.SelectMany(project => Enumerable.Range(1, 2).Select(index =>
        {
            var start = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-index * 28);
            return new SprintEntity
            {
                Name = $"Sprint {index}: {faker.Commerce.ProductAdjective()}",
                StartDate = start,
                EndDate = start.AddDays(13),
                ProjectId = project.Id,
                Project = project,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        })).ToList();
        dbContext.Sprints.AddRange(sprints);
        await dbContext.SaveChangesAsync(cancellationToken);

        long friendlyId = 1;
        var tasks = context.Projects.SelectMany(project => Enumerable.Range(0, 6).Select(_ =>
        {
            var status = faker.PickRandom<TaskStatus>();
            var assignedUser = project.ProjectMembers.Count == 0 ? (Guid?) null : faker.PickRandom(project.ProjectMembers).UserId;
            return new TaskEntity
            {
                Name = $"{faker.Hacker.Verb()} {faker.Hacker.Noun()}",
                Description = faker.Lorem.Sentence(12),
                Status = status,
                Priority = faker.PickRandom<TaskPriority>(),
                FinishedDate = status == TaskStatus.Done ? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)) : null,
                FriendlyId = friendlyId++,
                AssignedUserId = assignedUser,
                ProjectId = project.Id,
                Project = project,
                SprintId = faker.PickRandom(sprints.Where(sprint => sprint.ProjectId == project.Id)).Id,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        })).ToList();
        dbContext.Tasks.AddRange(tasks);
        await dbContext.SaveChangesAsync(cancellationToken);
        context.Sprints.AddRange(sprints);
        context.Tasks.AddRange(tasks);
    }
}
