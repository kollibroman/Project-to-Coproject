using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Management.Models;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Management.Queries;

public record GetTasksFromSprintQuery : IStreamRequest<GetTasksFromSprintQuery, TaskDto>
{
    public required Guid SprintId { get; init; }
}

internal class GetTasksFromSprintQueryHandler : IStreamRequestHandler<GetTasksFromSprintQuery, TaskDto>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public GetTasksFromSprintQueryHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IAsyncEnumerable<TaskDto> Handle(GetTasksFromSprintQuery request, CancellationToken cancellationToken)
    {
        return _dbContext.Tasks
            .Where(x => x.SprintId == request.SprintId)
            .Select(x => new TaskDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Status = x.Status,
                Priority = x.Priority,
                FinishedDate = x.FinishedDate,
                SprintId = x.SprintId,
                AssignedUserId = x.AssignedUserId
            })
            .AsAsyncEnumerable();
    }
}
