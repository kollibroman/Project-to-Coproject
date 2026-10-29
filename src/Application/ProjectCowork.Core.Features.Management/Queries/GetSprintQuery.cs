using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Management.Models;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Management.Queries;

public record GetSprintQuery : IRequest<GetSprintQuery, Task<SprintDto>>
{
    public required Guid SprintId { get; init; }
}

internal class GetSprintQueryHandler : IRequestHandler<GetSprintQuery, Task<SprintDto>>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public GetSprintQueryHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task<SprintDto> Handle(GetSprintQuery request, CancellationToken ct)
    {
        var result = await _dbContext.Sprints
            .Where(x => x.Id == request.SprintId)
            .Select(x => new
            {
                x.ProjectId,
                Sprint = new SprintDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate
                }
            })
            .FirstOrDefaultAsync(ct);

        if (result is null)
        {
            throw new EntityNotFoundException($"Sprint {request.SprintId} does not exist");
        }

        await _permissionService.EnsureProjectPermissionAsync(result.ProjectId, ResourceType.Management, ActionType.View, ct);

        return result.Sprint;
    }
}
