using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Models;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Queries;

public record GetDiscoverProjectsQuery : IStreamRequest<GetDiscoverProjectsQuery, ProjectModel>;

internal class GetDiscoverProjectsQueryHandler : IStreamRequestHandler<GetDiscoverProjectsQuery, ProjectModel>
{
    private readonly ProjectCoworkDbContext _context;
    private readonly IAuthorizedUserProvider _authorizedUserProvider;

    public GetDiscoverProjectsQueryHandler(ProjectCoworkDbContext context, IAuthorizedUserProvider authorizedUserProvider)
    {
        _context = context;
        _authorizedUserProvider = authorizedUserProvider;
    }

    // Projekty, do których bieżący użytkownik nie należy (nie jest właścicielem ani współpracownikiem).
    public IAsyncEnumerable<ProjectModel> Handle(GetDiscoverProjectsQuery request, CancellationToken cancellationToken)
    {
        var userId = _authorizedUserProvider.GetCurrentUserId();

        return _context.Projects
            .Where(x => x.OwnerId != userId)
            .Where(x => x.Collaborators.All(c => c.Id != userId))
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
