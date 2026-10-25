using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Management.Models;
using ProjectCowork.Domain.Enums.Management;
using ProjectCowork.Persistence;
using TaskStatus = ProjectCowork.Domain.Enums.Management.TaskStatus;

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