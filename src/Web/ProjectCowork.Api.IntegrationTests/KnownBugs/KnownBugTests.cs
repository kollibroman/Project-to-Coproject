using System.Net;
using Microsoft.AspNetCore.Mvc;
using ProjectCowork.Api.IntegrationTests.Infrastructure;
using ProjectCowork.Api.Models;

namespace ProjectCowork.Api.IntegrationTests.KnownBugs;

[ClassDataSource<ApiFactory>(Shared = SharedType.PerTestSession)]
public class KnownBugTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Test]
    [Skip("Znany błąd. Dziś 500: GetCurrentUserId rzuca UnauthorizedAccessException, którego GlobalExceptionHandler nie mapuje.")]
    public async Task Anonymous_GetMyProjects_Returns401()
    {
        var response = await Anonymous().GetAsync("/api/projects");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Skip("Znany błąd. Dziś 500: LoginCommand rzuca UnauthorizedAccessException zamiast wyjątku z ExceptionBase.")]
    public async Task Login_WrongPassword_Returns401()
    {
        var user = await RegisterAsync();

        var response = await LoginAsync(user.Email, "ZleHaslo123");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Skip("Znany błąd. Dziś 500: błędy Identity są opakowane w InvalidOperationException.")]
    public async Task Register_TooShortPassword_Returns400()
    {
        var userName = Unique("u");

        var response = await Anonymous().PostAsJsonAsync("/api/auth/register/user", new RegisterRequest
        {
            Email = $"{userName}@example.com",
            Username = userName,
            Password = "Ab1!"
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    [Skip("Znany błąd. Dziś 200: Identity nie ma RequireUniqueEmail, a logowanie przez FindByEmailAsync nie obsłuży dwóch kont z tym samym e-mailem.")]
    public async Task Register_DuplicateEmail_IsRejected()
    {
        var existing = await RegisterAsync();

        var response = await Anonymous().PostAsJsonAsync("/api/auth/register/user", new RegisterRequest
        {
            Email = existing.Email,
            Username = Unique("u"),
            Password = DefaultPassword
        });

        await Assert.That((int)response.StatusCode).IsGreaterThanOrEqualTo(400).And.IsLessThan(500);
    }

    [Test]
    [Skip("Znany błąd. Dziś 500: SprintAggregate rzuca ArgumentException.")]
    public async Task AddSprint_EndBeforeStart_Returns400()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await owner.Client.PostAsJsonAsync("/api/sprints", new AddSprintRequest
        {
            ProjectId = project.Id,
            Name = "Sprint",
            StartDate = new DateOnly(2026, 11, 14),
            EndDate = new DateOnly(2026, 11, 1)
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    [Skip("Znany błąd. Dziś 500: TaskAggregate rzuca InvalidOperationException.")]
    public async Task AddTask_SprintFromAnotherProject_Returns400()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var otherProject = await CreateProjectAsync(owner);
        var foreignSprint = await CreateSprintAsync(owner, otherProject.Id);

        var response = await PostTaskAsync(owner.Client, project.Id, Unique("Zadanie"), sprintId: foreignSprint.Id);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    [Skip("Znany błąd. Dziś 500: EntityNotFoundException bez podanego kodu ma domyślnie 500.")]
    public async Task EditSprint_NonExistent_Returns404()
    {
        var user = await RegisterAsync();

        var response = await user.Client.PutAsJsonAsync($"/api/sprints/{Guid.NewGuid()}", new EditSprintRequest
        {
            Name = "X",
            StartDate = new DateOnly(2026, 11, 1),
            EndDate = new DateOnly(2026, 11, 2)
        });

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    [Skip("Znany błąd. Dziś 500: EntityNotFoundException bez podanego kodu ma domyślnie 500.")]
    public async Task GetTask_NonExistent_Returns404()
    {
        var user = await RegisterAsync();

        var response = await user.Client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    [Skip("Znany błąd. Dziś 200: GET /api/permissions nie sprawdza zalogowania, a docs/uprawnienia-per-projekt.md mówi: tylko zalogowany.")]
    public async Task PermissionCatalog_Anonymous_Returns401()
    {
        var response = await Anonymous().GetAsync("/api/permissions");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Skip("Znany błąd. GlobalExceptionHandler wpisuje StackTrace do ProblemDetails.Detail.")]
    public async Task ErrorResponse_DoesNotLeakStackTrace()
    {
        var user = await RegisterAsync();

        var response = await user.Client.GetAsync($"/api/projects/{Guid.NewGuid()}");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        await Assert.That(problem!.Detail ?? string.Empty).DoesNotContain(" at ");
    }

    [Test]
    [Skip("Znany błąd. Równoległe żądania dostają ten sam FriendlyId: brak blokady, concurrency tokena i unikalnego indeksu (ProjectId, FriendlyId).")]
    public async Task ConcurrentTaskCreation_GivesUniqueFriendlyIds()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var responses = await Task.WhenAll(Enumerable.Range(0, 10)
            .Select(i => PostTaskAsync(owner.Client, project.Id, $"Równoległe {i}")));

        var statuses = responses.Select(x => (int)x.StatusCode).ToList();
        var friendlyIds = (await GetTasksAsync(owner, project.Id)).Select(x => x.FriendlyId).ToList();

        await Assert.That(statuses).All().Satisfy(x => x.IsEqualTo((int)HttpStatusCode.Created));
        await Assert.That(friendlyIds.Distinct().Count()).IsEqualTo(friendlyIds.Count);
    }
}
