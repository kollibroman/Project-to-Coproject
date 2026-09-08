using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.ProjectPosting.Models;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.ProjectPosting.Queries;

public record GetProjectPostingsQuery : IStreamRequest<GetProjectPostingsQuery, ProjectPostingDto>
{
    public required Guid ProjectId { get; init; }
}

internal class GetProjectPostingsQueryHandler : IStreamRequestHandler<GetProjectPostingsQuery, ProjectPostingDto>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public GetProjectPostingsQueryHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IAsyncEnumerable<ProjectPostingDto> Handle(GetProjectPostingsQuery request, CancellationToken cancellationToken)
    {
        return _dbContext.ProjectPostings
            .Where(x => x.ProjectId == request.ProjectId)
            .Select(x => new ProjectPostingDto
            {
                Id = x.Id,
                JobDescription = x.JobDescription,
                ProjectDescription = x.ProjectDescription,
                ProjectId = x.ProjectId
            })
            .AsAsyncEnumerable();
    }
}