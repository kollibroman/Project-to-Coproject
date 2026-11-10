using DispatchR.Abstractions.Notification;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Infrastructure.DomainEvents.Projects;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.EventHandlers;

public class InvitationAcceptedDomainEventHandler : INotificationHandler<InvitationAcceptedEvent>
{
    private readonly  ProjectCoworkDbContext _dbContext;

    public InvitationAcceptedDomainEventHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask Handle(InvitationAcceptedEvent request, CancellationToken ct)
    {
        var invitation = await _dbContext.ProjectInvitations
            .FirstOrDefaultAsync(x => x.Id == request.InvitationId, ct);

        if (invitation == null)
        {
            throw new EntityNotFoundException("Invitation not found");
        }
        
        var project = await _dbContext.Projects
            .IncludeAccessGraph()
            .FirstOrDefaultAsync(x => x.Id == invitation.ProjectId, ct);

        if (project == null)
        {
            throw new EntityNotFoundException("Project not found");
        }

        var acceptingUser = await _dbContext.Users
            .Where(x => x.Id == invitation.UserId)
            .FirstOrDefaultAsync(ct);

        if (acceptingUser == null)
        {
            throw  new EntityNotFoundException("User not found");
        }
        
        new ProjectAggregate(project).AddMember(acceptingUser, invitation.RoleId);
        
        await _dbContext.SaveChangesAsync(ct);
    }
}