using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using ProjectCowork.Api.Models;
using ProjectCowork.Core.Features.Management.Models;
using ProjectCowork.Core.Features.Models;
using ProjectCowork.Core.Features.Permissions.Models;
using ProjectCowork.Core.Features.Repositories.Models;
using ProjectCowork.Domain.Enums.Management;
using TaskStatus = ProjectCowork.Domain.Enums.Management.TaskStatus;

namespace ProjectCowork.Api.IntegrationTests.Infrastructure;

public abstract class ApiTestBase(ApiFactory factory)
{
    private const string RefreshCookieName = "pc_refresh_token";
    protected const string DefaultPassword = "Test1234!";

    protected const string AdministratorRole = "Administrator";
    protected const string MemberRole = "Członek";
    protected const string ObserverRole = "Obserwator";

    protected ApiFactory Factory { get; } = factory;

    protected HttpClient Anonymous() => Factory.CreateApiClient();

    protected static string Unique(string prefix) => $"{prefix}_{Guid.NewGuid():N}"[..(prefix.Length + 13)];

    protected async Task<TestUser> RegisterAsync(string kind = "user")
    {
        var userName = Unique("u");
        var email = $"{userName}@example.com";

        var response = await Anonymous().PostAsJsonAsync($"/api/auth/register/{kind}", new RegisterRequest
        {
            Email = email,
            Username = userName,
            Password = DefaultPassword
        });

        await EnsureStatusAsync(response, HttpStatusCode.OK);

        var token = await ReadAccessTokenAsync(response);

        return new TestUser
        {
            Id = GetUserId(token),
            Email = email,
            UserName = userName,
            Password = DefaultPassword,
            AccessToken = token,
            RefreshToken = GetRefreshCookie(response)
                           ?? throw new InvalidOperationException("Register did not set refresh cookie."),
            Client = Authorized(token)
        };
    }

    protected HttpClient Authorized(string accessToken)
    {
        var client = Factory.CreateApiClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    protected Task<HttpResponseMessage> LoginAsync(string email, string password) =>
        Anonymous().PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = password });

    protected Task<HttpResponseMessage> PostWithRefreshCookieAsync(string path, string? refreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path);
        if (refreshToken is not null)
        {
            request.Headers.Add("Cookie", $"{RefreshCookieName}={refreshToken}");
        }

        return Anonymous().SendAsync(request);
    }

    protected static async Task<string> ReadAccessTokenAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
        return body?.AccessToken ?? throw new InvalidOperationException("Response has no access token.");
    }

    protected static string? GetRefreshCookie(HttpResponseMessage response) =>
        GetSetCookieHeader(response)?.Split(';')[0][(RefreshCookieName.Length + 1)..];

    protected static string? GetSetCookieHeader(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.FirstOrDefault(x => x.StartsWith($"{RefreshCookieName}=", StringComparison.Ordinal))
            : null;

    protected static JwtSecurityToken ReadJwt(string token) => new JwtSecurityTokenHandler().ReadJwtToken(token);

    private static Guid GetUserId(string token)
    {
        var claim = ReadJwt(token).Claims.First(x => x.Type is "nameid" or ClaimTypes.NameIdentifier);
        return Guid.Parse(claim.Value);
    }

    protected async Task<ProjectModel> CreateProjectAsync(TestUser owner)
    {
        var name = Unique("Projekt");

        var response = await owner.Client.PostAsJsonAsync("/api/projects",
            new CreateProjectRequest { Name = name, Description = "Opis projektu" });
        await EnsureStatusAsync(response, HttpStatusCode.Created);

        var projects = await GetListAsync<ProjectModel>(owner.Client, "/api/projects");
        return projects.Single(x => x.Name == name);
    }

    protected static async Task<List<ProjectRoleDto>> GetRolesAsync(HttpClient client, Guid projectId) =>
        await GetListAsync<ProjectRoleDto>(client, $"/api/projects/{projectId}/roles");

    protected static async Task<Guid> GetRoleIdAsync(TestUser owner, Guid projectId, string roleName) =>
        (await GetRolesAsync(owner.Client, projectId)).Single(x => x.Name == roleName).Id;

    protected static Task<HttpResponseMessage> InviteAsync(TestUser inviter, Guid projectId, string email,
        Guid? roleId = null) =>
        inviter.Client.PostAsJsonAsync($"/api/projects/{projectId}/invite", new InviteUserRequest
        {
            ProjectId = projectId,
            Email = email,
            RoleId = roleId,
            Message = "Dołącz do nas"
        });

    protected static async Task<ProjectInvitationModel> GetInvitationAsync(TestUser invitee, Guid projectId)
    {
        var invitations = await GetListAsync<ProjectInvitationModel>(invitee.Client, "/api/projects/invitations");
        return invitations.Single(x => x.ProjectId == projectId);
    }

    protected async Task AddMemberAsync(TestUser owner, Guid projectId, TestUser member, string roleName)
    {
        var roleId = await GetRoleIdAsync(owner, projectId, roleName);

        await EnsureStatusAsync(await InviteAsync(owner, projectId, member.Email, roleId), HttpStatusCode.NoContent);

        var invitation = await GetInvitationAsync(member, projectId);
        var accept = await member.Client.PostAsync($"/api/projects/invitations/{invitation.Id}/accept", null);
        await EnsureStatusAsync(accept, HttpStatusCode.NoContent);
    }

    protected static async Task<SprintDto> CreateSprintAsync(TestUser user, Guid projectId)
    {
        var name = Unique("Sprint");

        var response = await user.Client.PostAsJsonAsync("/api/sprints", new AddSprintRequest
        {
            ProjectId = projectId,
            Name = name,
            StartDate = new DateOnly(2026, 11, 1),
            EndDate = new DateOnly(2026, 11, 14)
        });
        await EnsureStatusAsync(response, HttpStatusCode.Created);

        var sprints = await GetListAsync<SprintDto>(user.Client, $"/api/sprints/project/{projectId}");
        return sprints.Single(x => x.Name == name);
    }

    protected static Task<HttpResponseMessage> PostTaskAsync(HttpClient client, Guid projectId, string name,
        TaskStatus status = TaskStatus.Todo, Guid? sprintId = null, Guid? assignedUserId = null) =>
        client.PostAsJsonAsync("/api/tasks", new AddTaskRequest
        {
            ProjectId = projectId,
            Name = name,
            Description = "Opis zadania",
            Priority = TaskPriority.Normal,
            Status = status,
            SprintId = sprintId,
            AssignedUserId = assignedUserId
        });

    protected static async Task<TaskDto> CreateTaskAsync(TestUser user, Guid projectId,
        TaskStatus status = TaskStatus.Todo, Guid? sprintId = null)
    {
        var name = Unique("Zadanie");

        await EnsureStatusAsync(await PostTaskAsync(user.Client, projectId, name, status, sprintId),
            HttpStatusCode.Created);

        return (await GetTasksAsync(user, projectId)).Single(x => x.Name == name);
    }

    protected static Task<List<TaskDto>> GetTasksAsync(TestUser user, Guid projectId) =>
        GetListAsync<TaskDto>(user.Client, $"/api/tasks/project/{projectId}");

    protected static Task<HttpResponseMessage> EditTaskAsync(TestUser user, TaskDto task,
        TaskStatus? status = null, Guid? sprintId = null, Guid? assignedUserId = null) =>
        user.Client.PutAsJsonAsync($"/api/tasks/{task.Id}", new EditTaskRequest
        {
            Name = task.Name,
            Description = task.Description,
            Priority = task.Priority,
            Status = status ?? task.Status,
            SprintId = sprintId,
            AssignedUserId = assignedUserId
        });

    protected static Task<HttpResponseMessage> PostRepositoryAsync(HttpClient client, Guid projectId, string name) =>
        client.PostAsJsonAsync("/api/git/repository/create", new CreateRepositoryRequest
        {
            Name = name,
            ProjectId = projectId
        });

    protected static async Task<RepositoryDto> CreateRepositoryAsync(TestUser user, Guid projectId,
        string? name = null)
    {
        name ??= Unique("repo");

        var response = await PostRepositoryAsync(user.Client, projectId, name);
        await EnsureStatusAsync(response, HttpStatusCode.OK);

        var repositories = await GetListAsync<RepositoryDto>(user.Client, $"/api/repository/project/{projectId}");
        return repositories.Single(x => x.Name == name);
    }

    protected static async Task<List<T>> GetListAsync<T>(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        await EnsureStatusAsync(response, HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<List<T>>() ?? [];
    }

    protected static async Task EnsureStatusAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        if (response.StatusCode != expected)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"{response.RequestMessage?.Method} {response.RequestMessage?.RequestUri?.PathAndQuery}: " +
                $"expected {(int)expected}, got {(int)response.StatusCode}. Body: {body}");
        }
    }
}
