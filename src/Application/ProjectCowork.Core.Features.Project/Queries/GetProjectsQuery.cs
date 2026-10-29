using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Models;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Queries;

/// <summary>
/// Projekty bieżącego użytkownika: te, których jest właścicielem, i te, w których jest członkiem.
/// </summary>
public record GetProjectsQuery : IStreamRequest<GetProjectsQuery, ProjectModel>;

internal class GetProjectsQueryHandler : IStreamRequestHandler<GetProjectsQuery, ProjectModel>
{
    private readonly ProjectCoworkDbContext _context;
    private readonly IAuthorizedUserProvider _authorizedUserProvider;

    public GetProjectsQueryHandler(ProjectCoworkDbContext context, IAuthorizedUserProvider authorizedUserProvider)
    {
        _context = context;
        _authorizedUserProvider = authorizedUserProvider;
    }

    // TODO: Pagination
    public IAsyncEnumerable<ProjectModel> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var userId = _authorizedUserProvider.GetCurrentUserId();

        return _context.Projects
            .Where(x => x.OwnerId == userId || x.ProjectMembers.Any(m => m.UserId == userId))
            .Select(x => new ProjectModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                OwnerId = x.OwnerId
            })
            .AsAsyncEnumerable();
    }
}
