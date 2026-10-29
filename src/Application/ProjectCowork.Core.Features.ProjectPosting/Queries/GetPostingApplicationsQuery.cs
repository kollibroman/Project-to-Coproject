using System.Runtime.CompilerServices;
using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.ProjectPosting.Models;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.ProjectPosting.Queries;

public record GetPostingApplicationsQuery : IStreamRequest<GetPostingApplicationsQuery, ProjectApplicationDto>
{
    public required Guid ProjectPostingId { get; init; }
}

internal class GetPostingApplicationQueryHandler : IStreamRequestHandler<GetPostingApplicationsQuery, ProjectApplicationDto>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public GetPostingApplicationQueryHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async IAsyncEnumerable<ProjectApplicationDto> Handle(GetPostingApplicationsQuery request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var projectId = await _dbContext.ProjectPostings
            .Where(x => x.Id == request.ProjectPostingId)
            .Select(x => (Guid?)x.ProjectId)
            .FirstOrDefaultAsync(cancellationToken);

        // Nieistniejące ogłoszenie daje pustą listę, tak jak wcześniej.
        if (projectId is null)
        {
            yield break;
        }

        await _permissionService.EnsureProjectPermissionAsync(projectId.Value, ResourceType.Applications,
            ActionType.View, cancellationToken);

        var applications = _dbContext.ProjectApplications
            .Where(x => x.ProjectPostingId == request.ProjectPostingId)
            .Select(x => new ProjectApplicationDto
            {
                Description = x.Description,
                UserId = x.UserId,
                Id = x.Id,
                UserEmail = x.User.Email
            })
            .AsAsyncEnumerable();

        await foreach (var application in applications.WithCancellation(cancellationToken))
        {
            yield return application;
        }
    }
}
