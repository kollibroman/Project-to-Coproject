using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Management.Models;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Management.Queries;

public record GetTasksFromProjectQuery : IStreamRequest<GetTasksFromProjectQuery, TaskDto>
{
    public required Guid ProjectId { get; init; }
}

internal class GetTasksFromProjectQueryHandler : IStreamRequestHandler<GetTasksFromProjectQuery, TaskDto>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public GetTasksFromProjectQueryHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IAsyncEnumerable<TaskDto> Handle(GetTasksFromProjectQuery request, CancellationToken cancellationToken)
    {
        return _dbContext.Tasks
            .Where(x => x.ProjectId == request.ProjectId)
            .Select(x => new TaskDto
            {

            })
            .AsAsyncEnumerable();
    }
}