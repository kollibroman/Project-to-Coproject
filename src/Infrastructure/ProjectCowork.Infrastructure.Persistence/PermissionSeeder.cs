using Bogus;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Models.Permissions;

namespace ProjectCowork.Persistence;

public static class PermissionSeeder
{
    private static readonly string[] FeatureNames =
    [
        "Projects",
        "Postings",
        "Repositories",
        "Applications",
        "Management",
        "Permissions"
    ];

    private static readonly string[] ActionNames =
    [
        "View",
        "Create",
        "Update",
        "Delete"
    ];

    public static async Task SeedAsync(ProjectCoworkDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var features = await dbContext.Features.ToListAsync(cancellationToken);
        var missingFeatures = FeatureNames
            .Where(name => features.All(feature => feature.Name != name))
            .Select(name => new Faker<FeatureEntity>()
                .RuleFor(feature => feature.Name, _ => name)
                .Generate())
            .ToList();

        if (missingFeatures.Count > 0)
        {
            dbContext.Features.AddRange(missingFeatures);
            await dbContext.SaveChangesAsync(cancellationToken);
            features.AddRange(missingFeatures);
        }

        var actions = await dbContext.Actions.ToListAsync(cancellationToken);
        var missingActions = ActionNames
            .Where(name => actions.All(action => action.Name != name))
            .Select(name => new Faker<ActionEntity>()
                .RuleFor(action => action.Name, _ => name)
                .Generate())
            .ToList();

        if (missingActions.Count > 0)
        {
            dbContext.Actions.AddRange(missingActions);
            await dbContext.SaveChangesAsync(cancellationToken);
            actions.AddRange(missingActions);
        }

        var existingPairs = await dbContext.FeatureActions
            .Select(permission => new { permission.FeatureId, permission.ActionId })
            .ToListAsync(cancellationToken);

        var missingPermissions =
            from feature in features.Where(feature => FeatureNames.Contains(feature.Name))
            from action in actions.Where(action => ActionNames.Contains(action.Name))
            where existingPairs.All(pair => pair.FeatureId != feature.Id || pair.ActionId != action.Id)
            select new Faker<FeatureActionEntity>()
                .RuleFor(permission => permission.FeatureId, _ => feature.Id)
                .RuleFor(permission => permission.ActionId, _ => action.Id)
                .Generate();

        dbContext.FeatureActions.AddRange(missingPermissions);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
