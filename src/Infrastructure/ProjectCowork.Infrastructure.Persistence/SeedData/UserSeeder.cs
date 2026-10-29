using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Models.Users;

namespace ProjectCowork.Persistence.SeedData;

internal static class UserSeeder
{
    public const string EmailPrefix = "seed.user";
    private const string Password = "Test123!";
    private const int RandomSeed = 20261029;

    public static async Task<SeedDataContext> SeedAsync(ProjectCoworkDbContext dbContext, CancellationToken cancellationToken)
    {
        var identityRoles = await EnsureRolesAsync(dbContext, cancellationToken);
        var faker = new Faker("pl");
        var passwordHasher = new PasswordHasher<UserEntity>();
        var users = Enumerable.Range(1, 8).Select(index =>
        {
            var username = $"{faker.Name.FirstName().ToLowerInvariant()}.{faker.Name.LastName().ToLowerInvariant()}{index}";
            var email = $"{EmailPrefix}{index}@example.com";
            var user = new UserEntity
            {
                UserName = username,
                NormalizedUserName = username.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            user.PasswordHash = passwordHasher.HashPassword(user, Password);
            return user;
        }).ToList();

        dbContext.Users.AddRange(users);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.UserRoles.AddRange(users.Select((user, index) => new IdentityUserRole<Guid>
        {
            UserId = user.Id,
            RoleId = identityRoles[index == 0 ? RoleNames.Admin : index == 1 ? RoleNames.Business : RoleNames.User].Id
        }));
        await dbContext.SaveChangesAsync(cancellationToken);

        var permissions = await dbContext.FeatureActions
            .Include(permission => permission.Feature)
            .Include(permission => permission.Action)
            .ToListAsync(cancellationToken);

        return new SeedDataContext
        {
            Users = users,
            Projects = [],
            ProjectRoles = [],
            Sprints = [],
            Tasks = [],
            Repositories = [],
            Postings = [],
            Applications = [],
            PermissionCatalog = permissions
        };
    }

    private static async Task<Dictionary<RoleNames, RoleEntity>> EnsureRolesAsync(
        ProjectCoworkDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var names = Enum.GetValues<RoleNames>();
        var existing = await dbContext.Roles.ToListAsync(cancellationToken);
        var result = new Dictionary<RoleNames, RoleEntity>();
        foreach (var name in names)
        {
            var role = existing.FirstOrDefault(item => item.Name == name.ToString());
            if (role is null)
            {
                role = new RoleEntity { Name = name.ToString(), NormalizedName = name.ToString().ToUpperInvariant() };
                dbContext.Roles.Add(role);
            }
            result[name] = role;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }
}
