using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Infrastructure.Domain.Tests.TestData;
using ProjectCowork.Infrastructure.DomainEvents.Management;

namespace ProjectCowork.Infrastructure.Domain.Tests.Aggregates;

public class SprintAggregateTests
{
    private static readonly DateOnly Start = new(2026, 11, 1);
    private static readonly DateOnly End = new(2026, 11, 14);

    [Test]
    public async Task Constructor_NullSprint_Throws()
    {
        await Assert.That(() => new SprintAggregate(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Create_SetsAllFields()
    {
        var projectId = Guid.NewGuid();

        var sprint = SprintAggregate.Create(projectId, "Sprint 1", Start, End);

        await Assert.That(sprint.Name).IsEqualTo("Sprint 1");
        await Assert.That(sprint.StartDate).IsEqualTo(Start);
        await Assert.That(sprint.EndDate).IsEqualTo(End);
        await Assert.That(sprint.ProjectId).IsEqualTo(projectId);
    }

    [Test]
    public async Task Create_OneDaySprint_IsAllowed()
    {
        var sprint = SprintAggregate.Create(Guid.NewGuid(), "Jednodniowy", Start, Start);

        await Assert.That(sprint.EndDate).IsEqualTo(sprint.StartDate);
    }

    [Test]
    public async Task Create_EndBeforeStart_Throws()
    {
        await Assert.That(() => SprintAggregate.Create(Guid.NewGuid(), "Sprint", End, Start))
            .Throws<ArgumentException>();
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task Create_EmptyName_Throws(string name)
    {
        await Assert.That(() => SprintAggregate.Create(Guid.NewGuid(), name, Start, End))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task SetName_ChangesName()
    {
        var sprint = TestEntities.Sprint(Guid.NewGuid());

        new SprintAggregate(sprint).SetName("Nowa nazwa");

        await Assert.That(sprint.Name).IsEqualTo("Nowa nazwa");
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task SetName_Empty_Throws_AndKeepsOldName(string name)
    {
        var sprint = TestEntities.Sprint(Guid.NewGuid());
        var aggregate = new SprintAggregate(sprint);

        await Assert.That(() => aggregate.SetName(name)).Throws<ArgumentException>();
        await Assert.That(sprint.Name).IsEqualTo("Sprint 1");
    }

    [Test]
    public async Task SetDates_ChangesDates()
    {
        var sprint = TestEntities.Sprint(Guid.NewGuid());
        var newStart = new DateOnly(2026, 12, 1);
        var newEnd = new DateOnly(2026, 12, 20);

        new SprintAggregate(sprint).SetDates(newStart, newEnd);

        await Assert.That(sprint.StartDate).IsEqualTo(newStart);
        await Assert.That(sprint.EndDate).IsEqualTo(newEnd);
    }

    [Test]
    public async Task SetDates_EndBeforeStart_Throws_AndKeepsOldDates()
    {
        var sprint = TestEntities.Sprint(Guid.NewGuid());
        var oldStart = sprint.StartDate;
        var oldEnd = sprint.EndDate;
        var aggregate = new SprintAggregate(sprint);

        await Assert.That(() => aggregate.SetDates(new DateOnly(2026, 12, 20), new DateOnly(2026, 12, 1)))
            .Throws<ArgumentException>();

        await Assert.That(sprint.StartDate).IsEqualTo(oldStart);
        await Assert.That(sprint.EndDate).IsEqualTo(oldEnd);
    }

    [Test]
    public async Task DeleteSprint_RaisesSprintDeletedEvent()
    {
        var projectId = Guid.NewGuid();
        var sprint = TestEntities.Sprint(projectId);
        var aggregate = new SprintAggregate(sprint);

        aggregate.DeleteSprint();

        var domainEvent = aggregate.DomainEvents.Single();
        await Assert.That(domainEvent).IsTypeOf<SprintDeletedEvent>();

        var deleted = (SprintDeletedEvent)domainEvent;
        await Assert.That(deleted.SprintId).IsEqualTo(sprint.Id);
        await Assert.That(deleted.ProjectId).IsEqualTo(projectId);
    }

    [Test]
    public async Task NewAggregate_HasNoDomainEvents()
    {
        var aggregate = new SprintAggregate(TestEntities.Sprint(Guid.NewGuid()));

        await Assert.That(aggregate.DomainEvents.Count).IsEqualTo(0);
    }
}
