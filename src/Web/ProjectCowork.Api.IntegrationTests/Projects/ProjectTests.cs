using System.Net;
using ProjectCowork.Api.IntegrationTests.Infrastructure;
using ProjectCowork.Api.Models;
using ProjectCowork.Core.Features.Models;
using ProjectCowork.Core.Features.Permissions.Models;
using ProjectCowork.Domain.Enums.Permissions;

namespace ProjectCowork.Api.IntegrationTests.Projects;

[ClassDataSource<ApiFactory>(Shared = SharedType.PerTestSession)]
public class ProjectTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Test]
    public async Task CreateProject_IsListedForOwnerWithOwnerRole()
    {
        var owner = await RegisterAsync();

        var project = await CreateProjectAsync(owner);

        await Assert.That(project.OwnerId).IsEqualTo(owner.Id);
        await Assert.That(project.RoleName).IsEqualTo("Owner");
        await Assert.That(project.Description).IsEqualTo("Opis projektu");
    }

    [Test]
    public async Task CreateProject_IsNotListedForOtherUser_ButIsInTheirDiscover()
    {
        var owner = await RegisterAsync();
        var other = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var otherProjects = await GetListAsync<ProjectModel>(other.Client, "/api/projects");
        var otherDiscover = await GetListAsync<ProjectModel>(other.Client, "/api/projects/discover");
        var ownerDiscover = await GetListAsync<ProjectModel>(owner.Client, "/api/projects/discover");

        await Assert.That(otherProjects.Any(x => x.Id == project.Id)).IsFalse();
        await Assert.That(otherDiscover.Any(x => x.Id == project.Id)).IsTrue();
        await Assert.That(ownerDiscover.Any(x => x.Id == project.Id)).IsFalse();
    }

    [Test]
    public async Task CreateProject_CreatesDefaultRolesFromPermissionCatalog()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var catalog = await GetListAsync<PermissionDto>(owner.Client, "/api/permissions");
        var roles = await GetRolesAsync(owner.Client, project.Id);

        var admin = roles.Single(x => x.Name == AdministratorRole);
        var member = roles.Single(x => x.Name == MemberRole);
        var observer = roles.Single(x => x.Name == ObserverRole);

        int[] Ids(Func<PermissionDto, bool> predicate) => catalog.Where(predicate).Select(x => x.Id).ToArray();

        await Assert.That(catalog.Count).IsEqualTo(
            Enum.GetValues<ResourceType>().Length * Enum.GetValues<ActionType>().Length);
        await Assert.That(roles.Count).IsEqualTo(3);
        await Assert.That(roles.Where(x => x.IsDefault).Select(x => x.Name)).IsEquivalentTo(new[] { MemberRole });

        await Assert.That(admin.PermissionIds).IsEquivalentTo(Ids(_ => true));
        await Assert.That(observer.PermissionIds).IsEquivalentTo(Ids(x => x.Action == ActionType.View));
        await Assert.That(member.PermissionIds).IsEquivalentTo(Ids(x =>
            x.Action == ActionType.View
            || x.Resource == ResourceType.Management
            || (x.Resource == ResourceType.Repositories && x.Action == ActionType.Create)));
    }

    [Test]
    public async Task GetProject_Owner_Returns200()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await owner.Client.GetAsync($"/api/projects/{project.Id}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ProjectModel>();
        await Assert.That(body!.Name).IsEqualTo(project.Name);
    }

    [Test]
    public async Task GetProject_Outsider_Returns403()
    {
        var owner = await RegisterAsync();
        var outsider = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await outsider.Client.GetAsync($"/api/projects/{project.Id}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetProject_NonExistent_Returns403()
    {
        var user = await RegisterAsync();

        var response = await user.Client.GetAsync($"/api/projects/{Guid.NewGuid()}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetProject_Anonymous_Returns401()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await Anonymous().GetAsync($"/api/projects/{project.Id}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task EditProject_Owner_ChangesNameAndDescription()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var newName = Unique("Zmieniony");

        var response = await owner.Client.PostAsJsonAsync($"/api/projects/{project.Id}/edit", new EditProjectRequest
        {
            ProjectId = project.Id,
            Name = newName,
            Description = "Nowy opis"
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        var updated = await owner.Client.GetFromJsonAsync<ProjectModel>($"/api/projects/{project.Id}");
        await Assert.That(updated!.Name).IsEqualTo(newName);
        await Assert.That(updated.Description).IsEqualTo("Nowy opis");
    }

    [Test]
    public async Task DeleteProject_Owner_RemovesProjectWithRolesAndMembers()
    {
        var owner = await RegisterAsync();
        var member = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await AddMemberAsync(owner, project.Id, member, MemberRole);
        await CreateSprintAsync(owner, project.Id);

        var response = await owner.Client.DeleteAsync($"/api/projects/{project.Id}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        var ownerProjects = await GetListAsync<ProjectModel>(owner.Client, "/api/projects");
        var memberProjects = await GetListAsync<ProjectModel>(member.Client, "/api/projects");
        await Assert.That(ownerProjects.Any(x => x.Id == project.Id)).IsFalse();
        await Assert.That(memberProjects.Any(x => x.Id == project.Id)).IsFalse();
    }

    [Test]
    public async Task DeleteProject_Member_Returns403_AndProjectStays()
    {
        var owner = await RegisterAsync();
        var member = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await AddMemberAsync(owner, project.Id, member, MemberRole);

        var response = await member.Client.DeleteAsync($"/api/projects/{project.Id}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        var ownerProjects = await GetListAsync<ProjectModel>(owner.Client, "/api/projects");
        await Assert.That(ownerProjects.Any(x => x.Id == project.Id)).IsTrue();
    }
}
