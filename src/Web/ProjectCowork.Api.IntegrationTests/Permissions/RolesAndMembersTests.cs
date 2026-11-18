using System.Net;
using ProjectCowork.Api.IntegrationTests.Infrastructure;
using ProjectCowork.Api.Models;
using ProjectCowork.Core.Features.Models;
using ProjectCowork.Core.Features.Permissions.Models;
using ProjectCowork.Domain.Enums.Permissions;

namespace ProjectCowork.Api.IntegrationTests.Permissions;

[ClassDataSource<ApiFactory>(Shared = SharedType.PerTestSession)]
public class RolesAndMembersTests(ApiFactory factory) : ApiTestBase(factory)
{
    private static Task<HttpResponseMessage> PostRoleAsync(TestUser user, Guid projectId, string name,
        IReadOnlyCollection<int> permissionIds, bool isDefault = false) =>
        user.Client.PostAsJsonAsync($"/api/projects/{projectId}/roles",
            new ProjectRoleRequest { Name = name, IsDefault = isDefault, PermissionIds = permissionIds });

    private static Task<HttpResponseMessage> PutRoleAsync(TestUser user, Guid projectId, Guid roleId, string name,
        IReadOnlyCollection<int> permissionIds, bool isDefault = false) =>
        user.Client.PutAsJsonAsync($"/api/projects/{projectId}/roles/{roleId}",
            new ProjectRoleRequest { Name = name, IsDefault = isDefault, PermissionIds = permissionIds });

    private static Task<HttpResponseMessage> ChangeRoleAsync(TestUser user, Guid projectId, Guid memberId,
        Guid roleId) =>
        user.Client.PutAsJsonAsync($"/api/projects/{projectId}/members/{memberId}",
            new ChangeMemberRoleRequest { RoleId = roleId });

    private static async Task<int> PermissionIdAsync(TestUser user, ResourceType resource, ActionType action)
    {
        var catalog = await GetListAsync<PermissionDto>(user.Client, "/api/permissions");
        return catalog.Single(x => x.Resource == resource && x.Action == action).Id;
    }
    
    [Test]
    public async Task CreateRole_IsReturnedWithPermissions()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var permissionId = await PermissionIdAsync(owner, ResourceType.Postings, ActionType.Create);
        var name = Unique("Rekruter");

        var response = await PostRoleAsync(owner, project.Id, name, [permissionId, permissionId]);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var role = (await GetRolesAsync(owner.Client, project.Id)).Single(x => x.Name == name);
        await Assert.That(role.PermissionIds).IsEquivalentTo(new[] { permissionId });
        await Assert.That(role.IsDefault).IsFalse();
        await Assert.That(role.MemberCount).IsEqualTo(0);
    }

    [Test]
    public async Task CreateRole_AsDefault_MovesDefaultFlag()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var name = Unique("Nowa");

        await EnsureStatusAsync(await PostRoleAsync(owner, project.Id, name, [], isDefault: true),
            HttpStatusCode.Created);

        var roles = await GetRolesAsync(owner.Client, project.Id);
        await Assert.That(roles.Where(x => x.IsDefault).Select(x => x.Name)).IsEquivalentTo(new[] { name });
    }

    [Test]
    [Arguments("Członek")]
    [Arguments("  administrator ")]
    public async Task CreateRole_DuplicateName_Returns409(string name)
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await PostRoleAsync(owner, project.Id, name, []);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task CreateRole_UnknownPermissionId_Returns400()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await PostRoleAsync(owner, project.Id, Unique("Rola"), [int.MaxValue]);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateRole_EmptyName_Returns400()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await PostRoleAsync(owner, project.Id, "   ", []);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task UpdateRole_ReplacesWholePermissionSet()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var observerId = await GetRoleIdAsync(owner, project.Id, ObserverRole);
        var permissionId = await PermissionIdAsync(owner, ResourceType.Management, ActionType.Create);

        var response = await PutRoleAsync(owner, project.Id, observerId, ObserverRole, [permissionId]);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
        var role = (await GetRolesAsync(owner.Client, project.Id)).Single(x => x.Id == observerId);
        await Assert.That(role.PermissionIds).IsEquivalentTo(new[] { permissionId });
    }

    [Test]
    public async Task UpdateRole_UnsetDefaultOnDefaultRole_Returns400()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var memberRoleId = await GetRoleIdAsync(owner, project.Id, MemberRole);

        var response = await PutRoleAsync(owner, project.Id, memberRoleId, MemberRole, [], isDefault: false);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task UpdateRole_RoleFromAnotherProject_Returns404()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var otherProject = await CreateProjectAsync(owner);
        var foreignRoleId = await GetRoleIdAsync(owner, otherProject.Id, ObserverRole);

        var response = await PutRoleAsync(owner, project.Id, foreignRoleId, "X", []);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteRole_WithoutMembers_Returns204()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var observerId = await GetRoleIdAsync(owner, project.Id, ObserverRole);

        var response = await owner.Client.DeleteAsync($"/api/projects/{project.Id}/roles/{observerId}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
        var roles = await GetRolesAsync(owner.Client, project.Id);
        await Assert.That(roles.Any(x => x.Id == observerId)).IsFalse();
    }

    [Test]
    public async Task DeleteRole_Default_Returns409()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var memberRoleId = await GetRoleIdAsync(owner, project.Id, MemberRole);

        var response = await owner.Client.DeleteAsync($"/api/projects/{project.Id}/roles/{memberRoleId}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task DeleteRole_WithMembers_Returns409()
    {
        var owner = await RegisterAsync();
        var member = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await AddMemberAsync(owner, project.Id, member, ObserverRole);
        var observerId = await GetRoleIdAsync(owner, project.Id, ObserverRole);

        var response = await owner.Client.DeleteAsync($"/api/projects/{project.Id}/roles/{observerId}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
        var role = (await GetRolesAsync(owner.Client, project.Id)).Single(x => x.Id == observerId);
        await Assert.That(role.MemberCount).IsEqualTo(1);
    }
    
    [Test]
    public async Task ChangeMemberRole_GrantsNewPermissionsImmediately()
    {
        var owner = await RegisterAsync();
        var member = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await AddMemberAsync(owner, project.Id, member, ObserverRole);

        var before = await PostTaskAsync(member.Client, project.Id, Unique("Zadanie"));
        await Assert.That(before.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        var adminId = await GetRoleIdAsync(owner, project.Id, AdministratorRole);
        var change = await ChangeRoleAsync(owner, project.Id, member.Id, adminId);
        await Assert.That(change.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        var after = await PostTaskAsync(member.Client, project.Id, Unique("Zadanie"));
        await Assert.That(after.StatusCode).IsEqualTo(HttpStatusCode.Created);
    }

    [Test]
    public async Task UpdateRolePermissions_AffectsExistingMembers()
    {
        var owner = await RegisterAsync();
        var member = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await AddMemberAsync(owner, project.Id, member, ObserverRole);
        var observerId = await GetRoleIdAsync(owner, project.Id, ObserverRole);
        var managementCreate = await PermissionIdAsync(owner, ResourceType.Management, ActionType.Create);

        await EnsureStatusAsync(await PutRoleAsync(owner, project.Id, observerId, ObserverRole, [managementCreate]),
            HttpStatusCode.NoContent);

        var response = await PostTaskAsync(member.Client, project.Id, Unique("Zadanie"));
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
    }

    [Test]
    public async Task ChangeMemberRole_Owner_Returns400()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var observerId = await GetRoleIdAsync(owner, project.Id, ObserverRole);

        var response = await ChangeRoleAsync(owner, project.Id, owner.Id, observerId);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ChangeMemberRole_RoleFromAnotherProject_Returns400()
    {
        var owner = await RegisterAsync();
        var member = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var otherProject = await CreateProjectAsync(owner);
        await AddMemberAsync(owner, project.Id, member, ObserverRole);
        var foreignAdminId = await GetRoleIdAsync(owner, otherProject.Id, AdministratorRole);

        var response = await ChangeRoleAsync(owner, project.Id, member.Id, foreignAdminId);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task RemoveMember_LosesAccessToProject()
    {
        var owner = await RegisterAsync();
        var member = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await AddMemberAsync(owner, project.Id, member, MemberRole);

        var response = await owner.Client.DeleteAsync($"/api/projects/{project.Id}/members/{member.Id}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        var projectResponse = await member.Client.GetAsync($"/api/projects/{project.Id}");
        var memberProjects = await GetListAsync<ProjectModel>(member.Client, "/api/projects");
        await Assert.That(projectResponse.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        await Assert.That(memberProjects.Any(x => x.Id == project.Id)).IsFalse();
    }

    [Test]
    public async Task RemoveMember_Owner_Returns400()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await owner.Client.DeleteAsync($"/api/projects/{project.Id}/members/{owner.Id}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task RemoveMember_NotAMember_Returns404()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await owner.Client.DeleteAsync($"/api/projects/{project.Id}/members/{Guid.NewGuid()}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }
}
