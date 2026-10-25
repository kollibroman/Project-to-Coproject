using DispatchR.Abstractions.Notification;
using Microsoft.Extensions.Logging;

namespace ProjectCowork.Infrastructure.DomainEvents.Repositories;

public sealed record RepositoryDeletedEvent : INotification
{
    public required Guid RepositoryId { get; init; }
    public required Guid ProjectId { get; init; }
    public required string RepositoryName { get; init; }
}

internal sealed class RepositoryDeletedEventHandler : INotificationHandler<RepositoryDeletedEvent>
{
    private readonly ILogger<RepositoryDeletedEventHandler> _logger;

    public RepositoryDeletedEventHandler(ILogger<RepositoryDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public ValueTask Handle(RepositoryDeletedEvent request, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: The layout is Repos/{projectId}/{repositoryName}.git, so this removes every repository
            // of the project, not only the deleted one. Kept as in the previous RepositoryAggregate implementation.
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Repos", $"{request.ProjectId}");

            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete files of repository {RepositoryId}", request.RepositoryId);
        }

        return ValueTask.CompletedTask;
    }
}
