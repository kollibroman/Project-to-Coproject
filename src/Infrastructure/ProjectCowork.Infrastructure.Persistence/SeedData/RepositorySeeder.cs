using Bogus;
using ProjectCowork.Domain.Models.Repositories;

namespace ProjectCowork.Persistence.SeedData;

internal static class RepositorySeeder
{
    public static async Task SeedAsync(ProjectCoworkDbContext dbContext, SeedDataContext context, CancellationToken cancellationToken)
    {
        var faker = new Faker("pl");
        var repositories = context.Projects.SelectMany(project => Enumerable.Range(1, 2).Select(index => new RepositoryEntity
        {
            RepositoryName = $"{faker.Hacker.Noun()}-{index}",
            RepoPath = $"https://github.com/projectcowork/{project.Id:N}-{index}",
            ProjectId = project.Id,
            Project = project,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        })).ToList();
        dbContext.Repositories.AddRange(repositories);
        await dbContext.SaveChangesAsync(cancellationToken);
        context.Repositories.AddRange(repositories);
    }
}
