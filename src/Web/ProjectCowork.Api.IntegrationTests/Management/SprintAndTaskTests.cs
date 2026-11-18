using System.Net;
using ProjectCowork.Api.IntegrationTests.Infrastructure;
using ProjectCowork.Api.Models;
using ProjectCowork.Core.Features.Management.Models;
using TaskStatus = ProjectCowork.Domain.Enums.Management.TaskStatus;

namespace ProjectCowork.Api.IntegrationTests.Management;

[ClassDataSource<ApiFactory>(Shared = SharedType.PerTestSession)]
public class SprintAndTaskTests(ApiFactory factory) : ApiTestBase(factory)
{
    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.UtcNow);

    [Test]
    public async Task CreateSprint_CanBeReadById()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var sprint = await CreateSprintAsync(owner, project.Id);

        var read = await owner.Client.GetFromJsonAsync<SprintDto>($"/api/sprints/{sprint.Id}");
        await Assert.That(read!.Name).IsEqualTo(sprint.Name);
        await Assert.That(read.StartDate).IsEqualTo(new DateOnly(2026, 11, 1));
        await Assert.That(read.EndDate).IsEqualTo(new DateOnly(2026, 11, 14));
    }

    [Test]
    public async Task SprintList_ContainsOnlySprintsOfThatProject()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var otherProject = await CreateProjectAsync(owner);
        var sprint = await CreateSprintAsync(owner, project.Id);
        var otherSprint = await CreateSprintAsync(owner, otherProject.Id);

        var sprints = await GetListAsync<SprintDto>(owner.Client, $"/api/sprints/project/{project.Id}");

        await Assert.That(sprints.Select(x => x.Id)).IsEquivalentTo(new[] { sprint.Id });
        await Assert.That(sprints.Any(x => x.Id == otherSprint.Id)).IsFalse();
    }

    [Test]
    public async Task EditSprint_ChangesNameAndDates()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var sprint = await CreateSprintAsync(owner, project.Id);

        var response = await owner.Client.PutAsJsonAsync($"/api/sprints/{sprint.Id}", new EditSprintRequest
        {
            Name = "Sprint 2",
            StartDate = new DateOnly(2026, 12, 1),
            EndDate = new DateOnly(2026, 12, 15)
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
        var read = await owner.Client.GetFromJsonAsync<SprintDto>($"/api/sprints/{sprint.Id}");
        await Assert.That(read!.Name).IsEqualTo("Sprint 2");
        await Assert.That(read.EndDate).IsEqualTo(new DateOnly(2026, 12, 15));
    }

    [Test]
    public async Task EditSprint_ByObserver_Returns403()
    {
        var owner = await RegisterAsync();
        var observer = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await AddMemberAsync(owner, project.Id, observer, ObserverRole);
        var sprint = await CreateSprintAsync(owner, project.Id);

        var response = await observer.Client.PutAsJsonAsync($"/api/sprints/{sprint.Id}", new EditSprintRequest
        {
            Name = "Przejęty",
            StartDate = sprint.StartDate,
            EndDate = sprint.EndDate
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task DeleteSprint_MovesItsTasksToBacklog()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var sprint = await CreateSprintAsync(owner, project.Id);
        var otherSprint = await CreateSprintAsync(owner, project.Id);
        var first = await CreateTaskAsync(owner, project.Id, sprintId: sprint.Id);
        var second = await CreateTaskAsync(owner, project.Id, sprintId: sprint.Id);
        var untouched = await CreateTaskAsync(owner, project.Id, sprintId: otherSprint.Id);

        var response = await owner.Client.DeleteAsync($"/api/sprints/{sprint.Id}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        var tasks = (await GetTasksAsync(owner, project.Id)).ToDictionary(x => x.Id);
        await Assert.That(tasks.Count).IsEqualTo(3);
        await Assert.That(tasks[first.Id].SprintId).IsNull();
        await Assert.That(tasks[second.Id].SprintId).IsNull();
        await Assert.That(tasks[untouched.Id].SprintId).IsEqualTo(otherSprint.Id);

        var sprints = await GetListAsync<SprintDto>(owner.Client, $"/api/sprints/project/{project.Id}");
        await Assert.That(sprints.Select(x => x.Id)).IsEquivalentTo(new[] { otherSprint.Id });
    }

    [Test]
    public async Task CreateTasks_GetConsecutiveFriendlyIdsPerProject()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var otherProject = await CreateProjectAsync(owner);

        var a = await CreateTaskAsync(owner, project.Id);
        var b = await CreateTaskAsync(owner, project.Id);
        var other = await CreateTaskAsync(owner, otherProject.Id);
        var c = await CreateTaskAsync(owner, project.Id);

        await Assert.That(new[] { a.FriendlyId, b.FriendlyId, c.FriendlyId }).IsEquivalentTo(new[] { 1L, 2L, 3L });
        await Assert.That(other.FriendlyId).IsEqualTo(1L);
    }

    [Test]
    public async Task CreateTask_Done_HasFinishedDate_AndReopeningClearsIt()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var task = await CreateTaskAsync(owner, project.Id, TaskStatus.Done);

        await Assert.That(task.FinishedDate).IsNotNull();
        await Assert.That(task.FinishedDate!.Value).IsGreaterThanOrEqualTo(Today().AddDays(-1));

        await EnsureStatusAsync(await EditTaskAsync(owner, task, TaskStatus.InProgress), HttpStatusCode.NoContent);

        var reopened = await owner.Client.GetFromJsonAsync<TaskDto>($"/api/tasks/{task.Id}");
        await Assert.That(reopened!.Status).IsEqualTo(TaskStatus.InProgress);
        await Assert.That(reopened.FinishedDate).IsNull();
    }

    [Test]
    public async Task EditTask_MoveToSprint_ThenBackToBacklog()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var sprint = await CreateSprintAsync(owner, project.Id);
        var task = await CreateTaskAsync(owner, project.Id);

        await EnsureStatusAsync(await EditTaskAsync(owner, task, sprintId: sprint.Id), HttpStatusCode.NoContent);

        var sprintTasks = await GetListAsync<TaskDto>(owner.Client, $"/api/sprints/{sprint.Id}/tasks");
        await Assert.That(sprintTasks.Select(x => x.Id)).IsEquivalentTo(new[] { task.Id });

        await EnsureStatusAsync(await EditTaskAsync(owner, task, sprintId: null), HttpStatusCode.NoContent);

        var afterBacklog = await GetListAsync<TaskDto>(owner.Client, $"/api/sprints/{sprint.Id}/tasks");
        await Assert.That(afterBacklog.Count).IsEqualTo(0);
    }

    [Test]
    public async Task EditTask_AssignAndUnassignUser()
    {
        var owner = await RegisterAsync();
        var member = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await AddMemberAsync(owner, project.Id, member, MemberRole);
        var task = await CreateTaskAsync(owner, project.Id);

        await EnsureStatusAsync(await EditTaskAsync(owner, task, assignedUserId: member.Id), HttpStatusCode.NoContent);
        var assigned = await owner.Client.GetFromJsonAsync<TaskDto>($"/api/tasks/{task.Id}");
        await Assert.That(assigned!.AssignedUserId).IsEqualTo(member.Id);

        await EnsureStatusAsync(await EditTaskAsync(owner, task, assignedUserId: null), HttpStatusCode.NoContent);
        var unassigned = await owner.Client.GetFromJsonAsync<TaskDto>($"/api/tasks/{task.Id}");
        await Assert.That(unassigned!.AssignedUserId).IsNull();
    }

    [Test]
    public async Task CreateTask_UnknownAssignee_Returns400()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await PostTaskAsync(owner.Client, project.Id, Unique("Zadanie"),
            assignedUserId: Guid.NewGuid());

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateTask_UnknownSprint_Returns400()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await PostTaskAsync(owner.Client, project.Id, Unique("Zadanie"), sprintId: Guid.NewGuid());

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task DeleteTask_RemovesIt()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var task = await CreateTaskAsync(owner, project.Id);

        var response = await owner.Client.DeleteAsync($"/api/tasks/{task.Id}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
        var tasks = await GetTasksAsync(owner, project.Id);
        await Assert.That(tasks.Any(x => x.Id == task.Id)).IsFalse();
    }

    [Test]
    public async Task GetTask_FromOtherProject_Returns403ForOutsider()
    {
        var owner = await RegisterAsync();
        var outsider = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var task = await CreateTaskAsync(owner, project.Id);

        var response = await outsider.Client.GetAsync($"/api/tasks/{task.Id}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }
}
