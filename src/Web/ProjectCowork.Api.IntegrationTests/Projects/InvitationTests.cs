using System.Net;
using ProjectCowork.Api.IntegrationTests.Infrastructure;
using ProjectCowork.Core.Features.Models;
using ProjectCowork.Core.Features.Permissions.Models;

namespace ProjectCowork.Api.IntegrationTests.Projects;

[ClassDataSource<ApiFactory>(Shared = SharedType.PerTestSession)]
public class InvitationTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Test]
    public async Task Invite_SendsEmail_AndInvitationIsVisibleToInvitee()
    {
        var owner = await RegisterAsync();
        var invitee = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await InviteAsync(owner, project.Id, invitee.Email);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
        await Assert.That(Factory.Mail.SentTo(invitee.Email).Count).IsEqualTo(1);

        var invitation = await GetInvitationAsync(invitee, project.Id);
        await Assert.That(invitation.ProjectName).IsEqualTo(project.Name);
        await Assert.That(invitation.InvitedBy).IsEqualTo(owner.UserName);
        await Assert.That(invitation.InvitationContent).IsEqualTo("Dołącz do nas");
        await Assert.That(invitation.RoleName).IsNull();
    }

    [Test]
    public async Task Invite_EmailIsCaseInsensitive()
    {
        var owner = await RegisterAsync();
        var invitee = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await InviteAsync(owner, project.Id, $"  {invitee.Email.ToUpperInvariant()} ");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task Accept_WithoutRole_AddsMemberWithDefaultRole()
    {
        var owner = await RegisterAsync();
        var invitee = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await EnsureStatusAsync(await InviteAsync(owner, project.Id, invitee.Email), HttpStatusCode.NoContent);
        var invitation = await GetInvitationAsync(invitee, project.Id);

        var response = await invitee.Client.PostAsync($"/api/projects/invitations/{invitation.Id}/accept", null);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        var inviteeProjects = await GetListAsync<ProjectModel>(invitee.Client, "/api/projects");
        await Assert.That(inviteeProjects.Single(x => x.Id == project.Id).RoleName).IsEqualTo(MemberRole);

        var pending = await GetListAsync<ProjectInvitationModel>(invitee.Client, "/api/projects/invitations");
        await Assert.That(pending.Any(x => x.Id == invitation.Id)).IsFalse();
    }

    [Test]
    public async Task Accept_WithRole_AddsMemberWithThatRole()
    {
        var owner = await RegisterAsync();
        var invitee = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        await AddMemberAsync(owner, project.Id, invitee, ObserverRole);

        var members = await GetListAsync<ProjectMemberDto>(owner.Client, $"/api/projects/{project.Id}/members");
        await Assert.That(members.Count).IsEqualTo(2);
        await Assert.That(members[0].IsOwner).IsTrue();
        await Assert.That(members[0].UserId).IsEqualTo(owner.Id);

        var member = members.Single(x => x.UserId == invitee.Id);
        await Assert.That(member.RoleName).IsEqualTo(ObserverRole);
        await Assert.That(member.IsOwner).IsFalse();
    }

    [Test]
    public async Task Decline_DoesNotAddMember_AndRemovesInvitation()
    {
        var owner = await RegisterAsync();
        var invitee = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await EnsureStatusAsync(await InviteAsync(owner, project.Id, invitee.Email), HttpStatusCode.NoContent);
        var invitation = await GetInvitationAsync(invitee, project.Id);

        var response = await invitee.Client.PostAsync($"/api/projects/invitations/{invitation.Id}/decline", null);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        var inviteeProjects = await GetListAsync<ProjectModel>(invitee.Client, "/api/projects");
        var pending = await GetListAsync<ProjectInvitationModel>(invitee.Client, "/api/projects/invitations");
        await Assert.That(inviteeProjects.Any(x => x.Id == project.Id)).IsFalse();
        await Assert.That(pending.Any(x => x.Id == invitation.Id)).IsFalse();
    }

    [Test]
    public async Task Decline_ThenInviteAgain_IsAllowed()
    {
        var owner = await RegisterAsync();
        var invitee = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await EnsureStatusAsync(await InviteAsync(owner, project.Id, invitee.Email), HttpStatusCode.NoContent);
        var invitation = await GetInvitationAsync(invitee, project.Id);
        await invitee.Client.PostAsync($"/api/projects/invitations/{invitation.Id}/decline", null);

        var response = await InviteAsync(owner, project.Id, invitee.Email);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task AnswerTwice_Returns409()
    {
        var owner = await RegisterAsync();
        var invitee = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await EnsureStatusAsync(await InviteAsync(owner, project.Id, invitee.Email), HttpStatusCode.NoContent);
        var invitation = await GetInvitationAsync(invitee, project.Id);
        await EnsureStatusAsync(
            await invitee.Client.PostAsync($"/api/projects/invitations/{invitation.Id}/accept", null),
            HttpStatusCode.NoContent);

        var second = await invitee.Client.PostAsync($"/api/projects/invitations/{invitation.Id}/decline", null);

        await Assert.That(second.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Accept_SomeoneElsesInvitation_Returns404_AndDoesNotAddMember()
    {
        var owner = await RegisterAsync();
        var invitee = await RegisterAsync();
        var intruder = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await EnsureStatusAsync(await InviteAsync(owner, project.Id, invitee.Email), HttpStatusCode.NoContent);
        var invitation = await GetInvitationAsync(invitee, project.Id);

        var response = await intruder.Client.PostAsync($"/api/projects/invitations/{invitation.Id}/accept", null);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        var intruderProjects = await GetListAsync<ProjectModel>(intruder.Client, "/api/projects");
        await Assert.That(intruderProjects.Any(x => x.Id == project.Id)).IsFalse();
    }

    [Test]
    public async Task Invite_UserWithPendingInvitation_Returns409()
    {
        var owner = await RegisterAsync();
        var invitee = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await EnsureStatusAsync(await InviteAsync(owner, project.Id, invitee.Email), HttpStatusCode.NoContent);

        var response = await InviteAsync(owner, project.Id, invitee.Email);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Invite_ExistingMember_Returns409()
    {
        var owner = await RegisterAsync();
        var member = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await AddMemberAsync(owner, project.Id, member, MemberRole);

        var response = await InviteAsync(owner, project.Id, member.Email);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Invite_Owner_Returns409()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await InviteAsync(owner, project.Id, owner.Email);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task Invite_UnknownEmail_Returns404()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await InviteAsync(owner, project.Id, $"{Unique("nobody")}@example.com");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Invite_ByMemberWithoutPermissionsCreate_Returns403()
    {
        var owner = await RegisterAsync();
        var member = await RegisterAsync();
        var invitee = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await AddMemberAsync(owner, project.Id, member, MemberRole);

        var response = await InviteAsync(member, project.Id, invitee.Email);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        await Assert.That(Factory.Mail.SentTo(invitee.Email).Count).IsEqualTo(0);
    }
}
