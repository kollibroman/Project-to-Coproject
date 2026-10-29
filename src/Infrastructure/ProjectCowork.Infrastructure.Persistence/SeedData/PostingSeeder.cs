using Bogus;
using ProjectCowork.Domain.Models.Attachments;
using ProjectCowork.Domain.Models.Posting;

namespace ProjectCowork.Persistence.SeedData;

internal static class PostingSeeder
{
    public static async Task SeedAsync(ProjectCoworkDbContext dbContext, SeedDataContext context, CancellationToken cancellationToken)
    {
        var faker = new Faker("pl");
        var postings = context.Projects.SelectMany(project => Enumerable.Range(1, 2).Select(_ => new ProjectPostingEntity
        {
            JobDescription = faker.Lorem.Paragraphs(2),
            ProjectDescription = project.Description,
            ProjectId = project.Id,
            Project = project,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        })).ToList();
        dbContext.ProjectPostings.AddRange(postings);
        await dbContext.SaveChangesAsync(cancellationToken);

        var applications = postings.Select((posting, index) =>
        {
            var user = context.Users[(index + 3) % context.Users.Count];
            return new ProjectApplicationEntity
            {
                Description = faker.Lorem.Paragraph(),
                UserId = user.Id,
                User = user,
                ProjectPostingId = posting.Id,
                ProjectPosting = posting,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        }).ToList();
        dbContext.ProjectApplications.AddRange(applications);
        await dbContext.SaveChangesAsync(cancellationToken);

        var attachments = applications.Take(4).Select((application, index) => new AttachmentEntity
        {
            FileName = $"resume-{index + 1}.pdf",
            PersistedFileName = $"seed-resume-{index + 1}.pdf",
            SizeInBytes = 120_000 + index * 10_000,
            ProjectApplicationId = application.Id,
            ProjectApplication = application,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        }).ToList();
        dbContext.Set<AttachmentEntity>().AddRange(attachments);
        await dbContext.SaveChangesAsync(cancellationToken);

        context.Postings.AddRange(postings);
        context.Applications.AddRange(applications);
    }
}
