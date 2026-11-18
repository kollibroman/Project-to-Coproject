using ProjectCowork.Domain.Enums.Management;
using ProjectCowork.Domain.Models.Project;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Infrastructure.Domain.Tests.TestData;
using TaskStatus = ProjectCowork.Domain.Enums.Management.TaskStatus;

namespace ProjectCowork.Infrastructure.Domain.Tests.Aggregates;

public class TaskAggregateTests
{
    private readonly ProjectEntity _project = TestEntities.Project();

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.UtcNow);

    [Test]
    public async Task Constructor_NullTask_Throws()
    {
        await Assert.That(() => new TaskAggregate(null!)).Throws<ArgumentNullException>();
    }

    // --- Create ---------------------------------------------------------------

    [Test]
    public async Task Create_SetsAllFields()
    {
        var sprint = TestEntities.Sprint(_project.Id);
        var assignee = Guid.NewGuid();

        var task = TaskAggregate.Create(_project, "Nazwa", "Opis", TaskPriority.Urgent, TaskStatus.InProgress,
            sprint, assignee);

        await Assert.That(task.Name).IsEqualTo("Nazwa");
        await Assert.That(task.Description).IsEqualTo("Opis");
        await Assert.That(task.Priority).IsEqualTo(TaskPriority.Urgent);
        await Assert.That(task.Status).IsEqualTo(TaskStatus.InProgress);
        await Assert.That(task.ProjectId).IsEqualTo(_project.Id);
        await Assert.That(task.SprintId).IsEqualTo(sprint.Id);
        await Assert.That(task.AssignedUserId).IsEqualTo(assignee);
    }

    [Test]
    public async Task Create_WithoutSprint_GoesToBacklog()
    {
        var task = TaskAggregate.Create(_project, "Nazwa", "Opis", TaskPriority.Normal, TaskStatus.Todo, null, null);

        await Assert.That(task.SprintId).IsNull();
        await Assert.That(task.AssignedUserId).IsNull();
    }

    [Test]
    public async Task Create_AssignsConsecutiveFriendlyIds()
    {
        var first = TaskAggregate.Create(_project, "A", "", TaskPriority.Normal, TaskStatus.Todo, null, null);
        var second = TaskAggregate.Create(_project, "B", "", TaskPriority.Normal, TaskStatus.Todo, null, null);
        var third = TaskAggregate.Create(_project, "C", "", TaskPriority.Normal, TaskStatus.Todo, null, null);

        await Assert.That(new[] { first.FriendlyId, second.FriendlyId, third.FriendlyId })
            .IsEquivalentTo(new[] { 1L, 2L, 3L });
        await Assert.That(_project.LastTaskFriendlyId).IsEqualTo(3L);
    }

    [Test]
    public async Task Create_ContinuesFromExistingCounter()
    {
        _project.LastTaskFriendlyId = 41;

        var task = TaskAggregate.Create(_project, "A", "", TaskPriority.Normal, TaskStatus.Todo, null, null);

        await Assert.That(task.FriendlyId).IsEqualTo(42L);
        await Assert.That(_project.LastTaskFriendlyId).IsEqualTo(42L);
    }

    [Test]
    public async Task Create_Done_SetsFinishedDateToToday()
    {
        var before = Today();

        var task = TaskAggregate.Create(_project, "A", "", TaskPriority.Normal, TaskStatus.Done, null, null);

        await Assert.That(task.FinishedDate).IsNotNull();
        await Assert.That(task.FinishedDate!.Value).IsGreaterThanOrEqualTo(before);
        await Assert.That(task.FinishedDate!.Value).IsLessThanOrEqualTo(Today());
    }

    [Test]
    [Arguments(TaskStatus.Todo)]
    [Arguments(TaskStatus.InProgress)]
    public async Task Create_NotDone_HasNoFinishedDate(TaskStatus status)
    {
        var task = TaskAggregate.Create(_project, "A", "", TaskPriority.Normal, status, null, null);

        await Assert.That(task.FinishedDate).IsNull();
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task Create_EmptyName_Throws_AndDoesNotConsumeFriendlyId(string name)
    {
        await Assert.That(() =>
                TaskAggregate.Create(_project, name, "", TaskPriority.Normal, TaskStatus.Todo, null, null))
            .Throws<ArgumentException>();

        await Assert.That(_project.LastTaskFriendlyId).IsEqualTo(0L);
    }

    [Test]
    public async Task Create_SprintFromAnotherProject_Throws_AndDoesNotConsumeFriendlyId()
    {
        var foreignSprint = TestEntities.Sprint(Guid.NewGuid());

        await Assert.That(() =>
                TaskAggregate.Create(_project, "A", "", TaskPriority.Normal, TaskStatus.Todo, foreignSprint, null))
            .Throws<InvalidOperationException>();

        await Assert.That(_project.LastTaskFriendlyId).IsEqualTo(0L);
    }

    // --- Setters --------------------------------------------------------------

    [Test]
    public async Task SetName_ChangesName()
    {
        var task = TestEntities.Task(_project.Id);

        new TaskAggregate(task).SetName("Nowa");

        await Assert.That(task.Name).IsEqualTo("Nowa");
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task SetName_Empty_Throws_AndKeepsOldName(string name)
    {
        var task = TestEntities.Task(_project.Id);
        var aggregate = new TaskAggregate(task);

        await Assert.That(() => aggregate.SetName(name)).Throws<ArgumentException>();
        await Assert.That(task.Name).IsEqualTo("Zadanie");
    }

    [Test]
    public async Task SetDescriptionAndPriority_ChangeValues()
    {
        var task = TestEntities.Task(_project.Id);
        var aggregate = new TaskAggregate(task);

        aggregate.SetDescription("Nowy opis");
        aggregate.SetPriority(TaskPriority.Critical);

        await Assert.That(task.Description).IsEqualTo("Nowy opis");
        await Assert.That(task.Priority).IsEqualTo(TaskPriority.Critical);
    }

    // --- SetStatus ------------------------------------------------------------

    [Test]
    public async Task SetStatus_ToDone_SetsFinishedDate()
    {
        var task = TestEntities.Task(_project.Id, TaskStatus.InProgress);

        new TaskAggregate(task).SetStatus(TaskStatus.Done);

        await Assert.That(task.Status).IsEqualTo(TaskStatus.Done);
        await Assert.That(task.FinishedDate).IsNotNull();
    }

    [Test]
    [Arguments(TaskStatus.Todo)]
    [Arguments(TaskStatus.InProgress)]
    public async Task SetStatus_FromDoneToOther_ClearsFinishedDate(TaskStatus status)
    {
        var task = TestEntities.Task(_project.Id, TaskStatus.Done);
        task.FinishedDate = new DateOnly(2026, 1, 1);

        new TaskAggregate(task).SetStatus(status);

        await Assert.That(task.Status).IsEqualTo(status);
        await Assert.That(task.FinishedDate).IsNull();
    }

    [Test]
    public async Task SetStatus_SameStatus_KeepsOriginalFinishedDate()
    {
        var finished = new DateOnly(2026, 1, 1);
        var task = TestEntities.Task(_project.Id, TaskStatus.Done);
        task.FinishedDate = finished;

        new TaskAggregate(task).SetStatus(TaskStatus.Done);

        await Assert.That(task.FinishedDate).IsEqualTo(finished);
    }

    // --- Sprint / backlog -----------------------------------------------------

    [Test]
    public async Task MoveToSprint_SameProject_SetsSprint()
    {
        var task = TestEntities.Task(_project.Id);
        var sprint = TestEntities.Sprint(_project.Id);

        new TaskAggregate(task).MoveToSprint(sprint);

        await Assert.That(task.SprintId).IsEqualTo(sprint.Id);
    }

    [Test]
    public async Task MoveToSprint_AnotherProject_Throws_AndKeepsOldSprint()
    {
        var currentSprint = TestEntities.Sprint(_project.Id);
        var task = TestEntities.Task(_project.Id, sprintId: currentSprint.Id);
        var aggregate = new TaskAggregate(task);

        await Assert.That(() => aggregate.MoveToSprint(TestEntities.Sprint(Guid.NewGuid())))
            .Throws<InvalidOperationException>();

        await Assert.That(task.SprintId).IsEqualTo(currentSprint.Id);
    }

    [Test]
    public async Task MoveToSprint_Null_Throws()
    {
        var aggregate = new TaskAggregate(TestEntities.Task(_project.Id));

        await Assert.That(() => aggregate.MoveToSprint(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task MoveToBacklog_ClearsSprint()
    {
        var sprint = TestEntities.Sprint(_project.Id);
        var task = TestEntities.Task(_project.Id, sprintId: sprint.Id);
        task.Sprint = sprint;

        new TaskAggregate(task).MoveToBacklog();

        await Assert.That(task.SprintId).IsNull();
        await Assert.That(task.Sprint).IsNull();
    }

    // --- Assignment -----------------------------------------------------------

    [Test]
    public async Task AssignUser_ThenUnassign()
    {
        var task = TestEntities.Task(_project.Id);
        var aggregate = new TaskAggregate(task);
        var userId = Guid.NewGuid();

        aggregate.AssignUser(userId);
        await Assert.That(task.AssignedUserId).IsEqualTo(userId);

        aggregate.Unassign();
        await Assert.That(task.AssignedUserId).IsNull();
    }
}
