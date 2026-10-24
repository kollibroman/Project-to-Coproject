using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.ProjectPosting.Models;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.ProjectPosting.Queries;

public record GetActivePostingsQuery : IStreamRequest<GetActivePostingsQuery, ProjectPostingDto>;

internal class GetActivePostingsQueryHandler : IStreamRequestHandler<GetActivePostingsQuery, ProjectPostingDto>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public GetActivePostingsQueryHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Globalna lista wszystkich aktywnych ofert ze wszystkich projektów.
    public IAsyncEnumerable<ProjectPostingDto> Handle(GetActivePostingsQuery request, CancellationToken cancellationToken)
    {
        return _dbContext.ProjectPostings
            .Where(x => x.IsActive)
            .Select(x => new ProjectPostingDto
            {
                Id = x.Id,
                JobDescription = x.JobDescription,
                ProjectDescription = x.ProjectDescription,
                ProjectId = x.ProjectId,
                IsActive = x.IsActive
            })
            .AsAsyncEnumerable();
    }
}
