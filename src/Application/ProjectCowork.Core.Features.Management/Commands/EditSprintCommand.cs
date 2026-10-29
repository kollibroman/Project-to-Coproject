using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Management.Commands;

public record EditSprintCommand : IRequest<EditSprintCommand, Task>
{
    public required Guid SprintId { get; init; }
    public required string Name { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
}

internal class EditSprintCommandHandler : IRequestHandler<EditSprintCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IPermissionService _permissionService;

    public EditSprintCommandHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task Handle(EditSprintCommand request, CancellationToken ct)
    {
        var sprint = await _dbContext.Sprints
            .FirstOrDefaultAsync(x => x.Id == request.SprintId, ct);

        if (sprint is null)
        {
            throw new EntityNotFoundException($"Sprint {request.SprintId} does not exist");
        }

        await _permissionService.EnsureProjectPermissionAsync(sprint.ProjectId, ResourceType.Management, ActionType.Update, ct);

        var aggregate = new SprintAggregate(sprint);

        aggregate.SetName(request.Name);
        aggregate.SetDates(request.StartDate, request.EndDate);

        await _dbContext.SaveChangesAsync(ct);
    }
}
