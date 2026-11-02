using System.Net;
using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Commands;

public record InviteUserToProjectCommand : IRequest<InviteUserToProjectCommand, Task>
{
    public required Guid ProjectId { get; init; }
    public required string Email { get; init; }

    /// <summary>Null oznacza rolę domyślną projektu.</summary>
    public Guid? RoleId { get; init; }
}

internal class InviteUserToProjectCommandHandler : IRequestHandler<InviteUserToProjectCommand, Task>
{
    private readonly ProjectCoworkDbContext _context;
    private readonly IPermissionService _permissionService;

    public InviteUserToProjectCommandHandler(ProjectCoworkDbContext context, IPermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    public async Task Handle(InviteUserToProjectCommand request, CancellationToken ct)
    {
        await _permissionService.EnsureProjectPermissionAsync(request.ProjectId, ResourceType.Permissions, ActionType.Create, ct);

        var project = await _context.Projects
            .IncludeAccessGraph()
            .FirstOrDefaultAsync(x => x.Id == request.ProjectId, ct);

        if (project is null)
        {
            throw new EntityNotFoundException($"Project {request.ProjectId} does not exist", HttpStatusCode.NotFound);
        }

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, ct);

        if (user is null)
        {
            throw new EntityNotFoundException($"User with email {request.Email} does not exist", HttpStatusCode.NotFound);
        }

        new ProjectAggregate(project).AddMember(user, request.RoleId);

        await _context.SaveChangesAsync(ct);
    }
}
