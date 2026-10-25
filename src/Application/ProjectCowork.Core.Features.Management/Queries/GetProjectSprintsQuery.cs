using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Management.Models;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Management.Queries;

public class GetProjectSprintsQuery : IStreamRequest<GetProjectSprintsQuery, SprintDto>
{
    public required Guid ProjectId { get; init; }
}

internal class GetProjectSprintsQueryHandler : IStreamRequestHandler<GetProjectSprintsQuery, SprintDto>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public GetProjectSprintsQueryHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IAsyncEnumerable<SprintDto> Handle(GetProjectSprintsQuery request, CancellationToken cancellationToken)
    {
        return _dbContext.Sprints
            .Where(x => x.ProjectId == request.ProjectId)
            .Select(x => new SprintDto
            {
                Id = x.Id,
                Name = x.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate
            })
            .AsAsyncEnumerable();
    }
}