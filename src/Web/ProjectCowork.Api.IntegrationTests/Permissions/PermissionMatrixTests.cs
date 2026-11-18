using System.Net;
using ProjectCowork.Api.IntegrationTests.Infrastructure;
using ProjectCowork.Api.Models;
using ProjectCowork.Core.Features.Models;

namespace ProjectCowork.Api.IntegrationTests.Permissions;

public enum Actor
{
    Owner,
    Administrator,
    Member,
    Observer,
    Outsider,
    Anonymous
}

[ClassDataSource<ApiFactory>(Shared = SharedType.PerTestSession)]
public class PermissionMatrixTests(ApiFactory factory) : ApiTestBase(factory)
{
    private sealed record Scenario(ProjectModel Project, Dictionary<Actor, TestUser> Users);

    private static readonly SemaphoreSlim ScenarioLock = new(1, 1);
    private static Scenario? _scenario;

    private async Task<Scenario> GetScenarioAsync()
    {
        await ScenarioLock.WaitAsync();
        try
        {
            if (_scenario is not null)
            {
                return _scenario;
            }

            var users = new Dictionary<Actor, TestUser>
            {
                [Actor.Owner] = await RegisterAsync(),
                [Actor.Administrator] = await RegisterAsync(),
                [Actor.Member] = await RegisterAsync(),
                [Actor.Observer] = await RegisterAsync(),
                [Actor.Outsider] = await RegisterAsync()
            };

            var owner = users[Actor.Owner];
            var project = await CreateProjectAsync(owner);

            await AddMemberAsync(owner, project.Id, users[Actor.Administrator], AdministratorRole);
            await AddMemberAsync(owner, project.Id, users[Actor.Member], MemberRole);
            await AddMemberAsync(owner, project.Id, users[Actor.Observer], ObserverRole);

            return _scenario = new Scenario(project, users);
        }
        finally
        {
            ScenarioLock.Release();
        }
    }

    private HttpClient ClientFor(Scenario scenario, Actor actor) =>
        actor == Actor.Anonymous ? Anonymous() : scenario.Users[actor].Client;

    [Test]
    [Arguments(Actor.Owner, HttpStatusCode.OK)]
    [Arguments(Actor.Administrator, HttpStatusCode.OK)]
    [Arguments(Actor.Member, HttpStatusCode.OK)]
    [Arguments(Actor.Observer, HttpStatusCode.OK)]
    [Arguments(Actor.Outsider, HttpStatusCode.Forbidden)]
    [Arguments(Actor.Anonymous, HttpStatusCode.Unauthorized)]
    public async Task GetProject_RequiresProjectsView(Actor actor, HttpStatusCode expected)
    {
        var scenario = await GetScenarioAsync();

        var response = await ClientFor(scenario, actor).GetAsync($"/api/projects/{scenario.Project.Id}");

        await Assert.That(response.StatusCode).IsEqualTo(expected);
    }

    [Test]
    [Arguments(Actor.Owner, HttpStatusCode.NoContent)]
    [Arguments(Actor.Administrator, HttpStatusCode.NoContent)]
    [Arguments(Actor.Member, HttpStatusCode.Forbidden)]
    [Arguments(Actor.Observer, HttpStatusCode.Forbidden)]
    [Arguments(Actor.Outsider, HttpStatusCode.Forbidden)]
    [Arguments(Actor.Anonymous, HttpStatusCode.Unauthorized)]
    public async Task EditProject_RequiresProjectsUpdate(Actor actor, HttpStatusCode expected)
    {
        var scenario = await GetScenarioAsync();
        var project = scenario.Project;

        var response = await ClientFor(scenario, actor).PostAsJsonAsync($"/api/projects/{project.Id}/edit",
            new EditProjectRequest { ProjectId = project.Id, Name = project.Name, Description = project.Description });

        await Assert.That(response.StatusCode).IsEqualTo(expected);
    }

    [Test]
    [Arguments(Actor.Owner, HttpStatusCode.OK)]
    [Arguments(Actor.Administrator, HttpStatusCode.OK)]
    [Arguments(Actor.Member, HttpStatusCode.OK)]
    [Arguments(Actor.Observer, HttpStatusCode.OK)]
    [Arguments(Actor.Outsider, HttpStatusCode.Forbidden)]
    [Arguments(Actor.Anonymous, HttpStatusCode.Unauthorized)]
    public async Task GetProjectTasks_RequiresManagementView(Actor actor, HttpStatusCode expected)
    {
        var scenario = await GetScenarioAsync();

        var response = await ClientFor(scenario, actor).GetAsync($"/api/tasks/project/{scenario.Project.Id}");

        await Assert.That(response.StatusCode).IsEqualTo(expected);
    }

    [Test]
    [Arguments(Actor.Owner, HttpStatusCode.Created)]
    [Arguments(Actor.Administrator, HttpStatusCode.Created)]
    [Arguments(Actor.Member, HttpStatusCode.Created)]
    [Arguments(Actor.Observer, HttpStatusCode.Forbidden)]
    [Arguments(Actor.Outsider, HttpStatusCode.Forbidden)]
    [Arguments(Actor.Anonymous, HttpStatusCode.Unauthorized)]
    public async Task AddSprint_RequiresManagementCreate(Actor actor, HttpStatusCode expected)
    {
        var scenario = await GetScenarioAsync();

        var response = await ClientFor(scenario, actor).PostAsJsonAsync("/api/sprints", new AddSprintRequest
        {
            ProjectId = scenario.Project.Id,
            Name = Unique("Sprint"),
            StartDate = new DateOnly(2026, 11, 1),
            EndDate = new DateOnly(2026, 11, 14)
        });

        await Assert.That(response.StatusCode).IsEqualTo(expected);
    }

    [Test]
    [Arguments(Actor.Owner, HttpStatusCode.Created)]
    [Arguments(Actor.Administrator, HttpStatusCode.Created)]
    [Arguments(Actor.Member, HttpStatusCode.Created)]
    [Arguments(Actor.Observer, HttpStatusCode.Forbidden)]
    [Arguments(Actor.Outsider, HttpStatusCode.Forbidden)]
    [Arguments(Actor.Anonymous, HttpStatusCode.Unauthorized)]
    public async Task AddTask_RequiresManagementCreate(Actor actor, HttpStatusCode expected)
    {
        var scenario = await GetScenarioAsync();

        var response = await PostTaskAsync(ClientFor(scenario, actor), scenario.Project.Id, Unique("Zadanie"));

        await Assert.That(response.StatusCode).IsEqualTo(expected);
    }

    [Test]
    [Arguments(Actor.Owner, HttpStatusCode.NoContent)]
    [Arguments(Actor.Administrator, HttpStatusCode.NoContent)]
    [Arguments(Actor.Member, HttpStatusCode.NoContent)]
    [Arguments(Actor.Observer, HttpStatusCode.Forbidden)]
    [Arguments(Actor.Outsider, HttpStatusCode.Forbidden)]
    [Arguments(Actor.Anonymous, HttpStatusCode.Unauthorized)]
    public async Task DeleteTask_RequiresManagementDelete(Actor actor, HttpStatusCode expected)
    {
        var scenario = await GetScenarioAsync();
        var task = await CreateTaskAsync(scenario.Users[Actor.Owner], scenario.Project.Id);

        var response = await ClientFor(scenario, actor).DeleteAsync($"/api/tasks/{task.Id}");

        await Assert.That(response.StatusCode).IsEqualTo(expected);
    }

    [Test]
    [Arguments(Actor.Owner, HttpStatusCode.OK)]
    [Arguments(Actor.Administrator, HttpStatusCode.OK)]
    [Arguments(Actor.Member, HttpStatusCode.OK)]
    [Arguments(Actor.Observer, HttpStatusCode.OK)]
    [Arguments(Actor.Outsider, HttpStatusCode.Forbidden)]
    [Arguments(Actor.Anonymous, HttpStatusCode.Unauthorized)]
    public async Task GetMembers_RequiresPermissionsView(Actor actor, HttpStatusCode expected)
    {
        var scenario = await GetScenarioAsync();

        var response = await ClientFor(scenario, actor).GetAsync($"/api/projects/{scenario.Project.Id}/members");

        await Assert.That(response.StatusCode).IsEqualTo(expected);
    }

    [Test]
    [Arguments(Actor.Owner, HttpStatusCode.Created)]
    [Arguments(Actor.Administrator, HttpStatusCode.Created)]
    [Arguments(Actor.Member, HttpStatusCode.Forbidden)]
    [Arguments(Actor.Observer, HttpStatusCode.Forbidden)]
    [Arguments(Actor.Outsider, HttpStatusCode.Forbidden)]
    [Arguments(Actor.Anonymous, HttpStatusCode.Unauthorized)]
    public async Task CreateRole_RequiresPermissionsCreate(Actor actor, HttpStatusCode expected)
    {
        var scenario = await GetScenarioAsync();

        var response = await ClientFor(scenario, actor).PostAsJsonAsync(
            $"/api/projects/{scenario.Project.Id}/roles",
            new ProjectRoleRequest { Name = Unique("Rola"), IsDefault = false, PermissionIds = [] });

        await Assert.That(response.StatusCode).IsEqualTo(expected);
    }
}
