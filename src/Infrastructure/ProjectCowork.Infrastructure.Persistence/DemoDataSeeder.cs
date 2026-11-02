using Microsoft.EntityFrameworkCore;
using ProjectCowork.Persistence.SeedData;

namespace ProjectCowork.Persistence;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(ProjectCoworkDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(user => user.Email!.StartsWith(UserSeeder.EmailPrefix), cancellationToken))
        {
            return;
        }

        var context = await UserSeeder.SeedAsync(dbContext, cancellationToken);
        await ProjectSeeder.SeedAsync(dbContext, context, cancellationToken);
        await ManagementSeeder.SeedAsync(dbContext, context, cancellationToken);
        await RepositorySeeder.SeedAsync(dbContext, context, cancellationToken);
        await PostingSeeder.SeedAsync(dbContext, context, cancellationToken);
    }
}
