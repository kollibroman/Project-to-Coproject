using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Models;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Queries;

public record GetProjectsQuery : IStreamRequest<GetProjectsQuery, ProjectModel>;

internal class GetProjectsQueryHandler : IStreamRequestHandler<GetProjectsQuery, ProjectModel>
{
    private readonly ProjectCoworkDbContext _context;

    public GetProjectsQueryHandler(ProjectCoworkDbContext context)
    {
        _context = context;
    }

    // TODO: Pagination
    public IAsyncEnumerable<ProjectModel> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        return _context.Projects
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