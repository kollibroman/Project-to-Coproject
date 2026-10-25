using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Management.Models;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Management.Queries;

public record GetTaskQuery : IRequest<GetTaskQuery, Task<TaskDto>>
{
    public required Guid TaskId { get; init; }
}

internal class GetTaskQueryHandler : IRequestHandler<GetTaskQuery, Task<TaskDto>>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public GetTaskQueryHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskDto> Handle(GetTaskQuery request, CancellationToken ct)
    {
        var task = await _dbContext.Tasks
            .Where(x => x.Id == request.TaskId)
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
            .FirstOrDefaultAsync(ct);

        if (task is null)
        {
            throw new EntityNotFoundException($"Task {request.TaskId} does not exist");
        }

        return task;
    }
}
