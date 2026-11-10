using ProjectCowork.Domain.Exceptions.Projects;
using ProjectCowork.Domain.Models.Project;
using ProjectCowork.Infrastructure.Domain.Abstractions;
using ProjectCowork.Infrastructure.DomainEvents.Projects;

namespace ProjectCowork.Infrastructure.Domain.Aggregates;

public class ProjectInvitationAggregate : AggregateRoot
{
    public ProjectInvitationEntity ProjectInvitation { get; }

    public ProjectInvitationAggregate(ProjectInvitationEntity projectInvitation)
    {
        ProjectInvitation = projectInvitation;
    }

    public static ProjectInvitationEntity Create(string invitationContent, Guid userId, Guid projectId, Guid? roleId)
    {
        return new ProjectInvitationEntity
        {
            InvitationContent = invitationContent,
            ProjectId = projectId,
            UserId = userId,
            IsActive = true,
            RoleId = roleId
        };
    }

    public void AcceptInvitation()
    {
        EnsureActive();

        ProjectInvitation.IsActive = false;
        ProjectInvitation.UserAccepted = true;
        
        RaiseDomainEvent(new InvitationAcceptedEvent
        {
            InvitationId = ProjectInvitation.Id
        });
    }

    public void DeclineInvitation()
    {
        EnsureActive();

        ProjectInvitation.IsActive = false;
        ProjectInvitation.UserAccepted = false;
    }

    private void EnsureActive()
    {
        if (!ProjectInvitation.IsActive)
        {
            throw new ProjectAccessException("Invitation has already been answered.");
        }
    }
}
