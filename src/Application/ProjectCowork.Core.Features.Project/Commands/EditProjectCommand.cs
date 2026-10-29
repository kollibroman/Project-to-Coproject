using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Commands;

public record EditProjectCommand : IRequest<EditProjectCommand, Task>
{
    public required Guid ProjectId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
}

internal class EditProjectCommandHandler : IRequestHandler<EditProjectCommand, Task>
{
    private readonly ProjectCoworkDbContext _context;
    private readonly IPermissionService _permissionService;

    public EditProjectCommandHandler(ProjectCoworkDbContext context, IPermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    public async Task Handle(EditProjectCommand request, CancellationToken ct)
    {
        await _permissionService.EnsureProjectPermissionAsync(request.ProjectId, ResourceType.Projects, ActionType.Update, ct);

        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == request.ProjectId, ct);

        if (project is null)
        {
            throw new EntityNotFoundException("Project not found");
        }

        var aggregate = new ProjectAggregate(project);
        
        aggregate.SetProjectName(request.Name);
        aggregate.SetDescription(request.Description);
        
        await _context.SaveChangesAsync(ct);
    }
}
