using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Commands;

public record DeleteProjectCommand : IRequest<DeleteProjectCommand, Task>
{
    public required Guid ProjectId { get; init; }
}

internal class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Task>
{
    private readonly ProjectCoworkDbContext _context;
    private readonly IPermissionService _permissionService;

    public DeleteProjectCommandHandler(ProjectCoworkDbContext context, IPermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    public async Task Handle(DeleteProjectCommand request, CancellationToken ct)
    {
        await _permissionService.EnsureProjectPermissionAsync(request.ProjectId, ResourceType.Projects, ActionType.Delete, ct);

        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == request.ProjectId, ct);

        if (project is null)
        {
            throw new EntityNotFoundException("Project not found");
        }

        var aggregate = new ProjectAggregate(project);
        
        aggregate.DeleteProject();

        _context.Remove(project);
        await _context.SaveChangesAsync(ct);
    }
}
