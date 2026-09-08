using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.ProjectPosting.Models;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.ProjectPosting.Queries;

public record GetProjectPostingQuery : IRequest<GetProjectPostingQuery, Task<ProjectPostingDto>>
{
    public required Guid ProjectPostingId { get; init; }
}

internal class GetProjectPostingQueryHandler : IRequestHandler<GetProjectPostingQuery, Task<ProjectPostingDto>>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public GetProjectPostingQueryHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProjectPostingDto> Handle(GetProjectPostingQuery request, CancellationToken ct)
    {
        return await _dbContext.ProjectPostings
            .Where(x => x.Id == request.ProjectPostingId)
            .Select(x => new ProjectPostingDto
            {
                Id = x.Id,
                JobDescription = x.JobDescription,
                ProjectDescription = x.ProjectDescription,
                ProjectId = x.ProjectId
            })
            .FirstAsync(ct);
    }
}