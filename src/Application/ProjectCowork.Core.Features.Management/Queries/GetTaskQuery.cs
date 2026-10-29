using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Management.Models;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Management.Queries;

public record GetTaskQuery : IRequest<GetTaskQuery, Task<TaskDto>>
{
    public required Guid TaskId { get; init; }
}

internal class GetTaskQueryHandler : IRequestHandler<GetTaskQuery, Task<TaskDto>>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public GetTaskQueryHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task<TaskDto> Handle(GetTaskQuery request, CancellationToken ct)
    {
        var result = await _dbContext.Tasks
            .Where(x => x.Id == request.TaskId)
            .Select(x => new
            {
                x.ProjectId,
                Task = new TaskDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Status = x.Status,
                    Priority = x.Priority,
                    FinishedDate = x.FinishedDate,
                    SprintId = x.SprintId,
                    AssignedUserId = x.AssignedUserId
                }
            })
            .FirstOrDefaultAsync(ct);

        if (result is null)
        {
            throw new EntityNotFoundException($"Task {request.TaskId} does not exist");
        }

        await _permissionService.EnsureProjectPermissionAsync(result.ProjectId, ResourceType.Management, ActionType.View, ct);

        return result.Task;
    }
}
