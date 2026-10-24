using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Commands;

public record InviteUserToProjectCommand : IRequest<InviteUserToProjectCommand, Task>
{
    public required Guid ProjectId { get; init; }
    public required string Email { get; init; }
}

internal class InviteUserToProjectCommandHandler : IRequestHandler<InviteUserToProjectCommand, Task>
{
    private readonly ProjectCoworkDbContext _context;

    public InviteUserToProjectCommandHandler(ProjectCoworkDbContext context)
    {
        _context = context;
    }

    public async Task Handle(InviteUserToProjectCommand request, CancellationToken ct)
    {
        var project = await _context.Projects
            .Include(x => x.Collaborators)
            .FirstOrDefaultAsync(x => x.Id == request.ProjectId, ct);

        if (project is null)
        {
            throw new EntityNotFoundException($"Project {request.ProjectId} does not exist");
        }

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, ct);

        if (user is null)
        {
            throw new EntityNotFoundException($"User with email {request.Email} does not exist");
        }

        var aggregate = new ProjectAggregate(project);

        aggregate.AddCollaborator(user);

        await _context.SaveChangesAsync(ct);
    }
}
