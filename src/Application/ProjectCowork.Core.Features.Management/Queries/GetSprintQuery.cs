using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Management.Models;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Management.Queries;

public record GetSprintQuery : IRequest<GetSprintQuery, Task<SprintDto>>
{
    public required Guid SprintId { get; init; }
}

internal class GetSprintQueryHandler : IRequestHandler<GetSprintQuery, Task<SprintDto>>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public GetSprintQueryHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SprintDto> Handle(GetSprintQuery request, CancellationToken ct)
    {
        var sprint = await _dbContext.Sprints
            .Where(x => x.Id == request.SprintId)
            .Select(x => new SprintDto
            {
                Id = x.Id,
                Name = x.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate
            })
            .FirstOrDefaultAsync(ct);

        if (sprint is null)
        {
            throw new EntityNotFoundException($"Sprint {request.SprintId} does not exist");
        }

        return sprint;
    }
}
