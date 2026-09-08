using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.ProjectPosting.Models;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.ProjectPosting.Queries;

public record GetPostingApplicationsQuery : IStreamRequest<GetPostingApplicationsQuery, ProjectApplicationDto>
{
    public required Guid ProjectPostingId { get; init; }
}

internal class GetPostingApplicationQueryHandler : IStreamRequestHandler<GetPostingApplicationsQuery, ProjectApplicationDto>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public GetPostingApplicationQueryHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IAsyncEnumerable<ProjectApplicationDto> Handle(GetPostingApplicationsQuery request, CancellationToken cancellationToken)
    {
        return _dbContext.ProjectApplications
            .Where(x => x.ProjectPostingId == request.ProjectPostingId)
            .Select(x => new ProjectApplicationDto
            {
                Description = x.Description,
                UserId = x.UserId,
                Id = x.Id
            })
            .AsAsyncEnumerable();
    }
}