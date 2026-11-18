using System.Net;
using ProjectCowork.Domain.Exceptions.Projects;
using ProjectCowork.Domain.Models.Project;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Infrastructure.Domain.Tests.TestData;
using ProjectCowork.Infrastructure.DomainEvents.Projects;

namespace ProjectCowork.Infrastructure.Domain.Tests.Aggregates;

public class ProjectInvitationAggregateTests
{
    private static ProjectInvitationEntity NewInvitation() =>
        ProjectInvitationAggregate.Create("Dołącz do nas", Guid.NewGuid(), Guid.NewGuid(), null).WithNewId();

    [Test]
    public async Task Create_SetsFields_AndIsActive()
    {
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var invitation = ProjectInvitationAggregate.Create("Treść", userId, projectId, roleId);

        await Assert.That(invitation.InvitationContent).IsEqualTo("Treść");
        await Assert.That(invitation.UserId).IsEqualTo(userId);
        await Assert.That(invitation.ProjectId).IsEqualTo(projectId);
        await Assert.That(invitation.RoleId).IsEqualTo(roleId);
        await Assert.That(invitation.IsActive).IsTrue();
        await Assert.That(invitation.UserAccepted).IsFalse();
    }

    [Test]
    public async Task AfterCreationProcess_RaisesInvitationCreatedEvent()
    {
        var invitation = NewInvitation();
        var aggregate = new ProjectInvitationAggregate(invitation);

        aggregate.AfterCreationProcess();

        var domainEvent = aggregate.DomainEvents.Single();
        await Assert.That(domainEvent).IsTypeOf<InvitationCreatedEvent>();
        await Assert.That(((InvitationCreatedEvent)domainEvent).InvitationId).IsEqualTo(invitation.Id);
    }

    [Test]
    public async Task AcceptInvitation_DeactivatesAndMarksAccepted_AndRaisesEvent()
    {
        var invitation = NewInvitation();
        var aggregate = new ProjectInvitationAggregate(invitation);

        aggregate.AcceptInvitation();

        await Assert.That(invitation.IsActive).IsFalse();
        await Assert.That(invitation.UserAccepted).IsTrue();

        var domainEvent = aggregate.DomainEvents.Single();
        await Assert.That(domainEvent).IsTypeOf<InvitationAcceptedEvent>();
        await Assert.That(((InvitationAcceptedEvent)domainEvent).InvitationId).IsEqualTo(invitation.Id);
    }

    [Test]
    public async Task DeclineInvitation_DeactivatesWithoutAccepting_AndRaisesNoEvent()
    {
        var invitation = NewInvitation();
        var aggregate = new ProjectInvitationAggregate(invitation);

        aggregate.DeclineInvitation();

        await Assert.That(invitation.IsActive).IsFalse();
        await Assert.That(invitation.UserAccepted).IsFalse();
        await Assert.That(aggregate.DomainEvents.Count).IsEqualTo(0);
    }

    [Test]
    [Arguments(true, true)]
    [Arguments(true, false)]
    [Arguments(false, true)]
    [Arguments(false, false)]
    public async Task AnsweringTwice_ThrowsConflict_AndKeepsFirstAnswer(bool firstAccept, bool secondAccept)
    {
        var invitation = NewInvitation();
        var aggregate = new ProjectInvitationAggregate(invitation);
        Answer(aggregate, firstAccept);
        var eventsAfterFirstAnswer = aggregate.DomainEvents.Count;

        var exception = await Assert.That(() => Answer(aggregate, secondAccept))
            .Throws<ProjectAccessException>();

        await Assert.That(exception!.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
        await Assert.That(invitation.UserAccepted).IsEqualTo(firstAccept);
        await Assert.That(aggregate.DomainEvents.Count).IsEqualTo(eventsAfterFirstAnswer);
    }

    private static void Answer(ProjectInvitationAggregate aggregate, bool accept)
    {
        if (accept)
        {
            aggregate.AcceptInvitation();
        }
        else
        {
            aggregate.DeclineInvitation();
        }
    }
}
