using System.Net;
using ProjectCowork.Api.IntegrationTests.Infrastructure;
using ProjectCowork.Core.Features.Repositories.Models;

namespace ProjectCowork.Api.IntegrationTests.KnownBugs;

[ClassDataSource<ApiFactory>(Shared = SharedType.PerTestSession)]
public class RepositoryKnownBugTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Test]
    [Skip("Znany błąd. Dziś 500: GitRepositoryPaths rzuca ArgumentException.")]
    public async Task Create_InvalidName_Returns400()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await PostRepositoryAsync(owner.Client, project.Id, "../ucieczka");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    [Skip("Znany błąd. Dziś 500: GitClientService rzuca InvalidOperationException.")]
    public async Task Create_DuplicateName_Returns409()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var repository = await CreateRepositoryAsync(owner, project.Id);

        var response = await PostRepositoryAsync(owner.Client, project.Id, repository.Name);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    [Skip("Znany błąd. Usunięcie jednego repozytorium kasuje z dysku wszystkie repozytoria projektu.")]
    public async Task Delete_OneRepository_KeepsOtherRepositoriesOfProject()
    {
        using var git = new GitCli();
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var toDelete = await CreateRepositoryAsync(owner, project.Id);
        var toKeep = await CreateRepositoryAsync(owner, project.Id);

        var work = await git.CloneAsync(toKeep.RepoUrl, "work");
        await git.RunAsync(work, "checkout", "-b", "main");
        await GitCli.WriteFileAsync(work, "README.md", "zostaję\n");
        await git.CommitAllAsync(work, "Commit");
        await git.RunAsync(work, "push", "origin", "main");

        await EnsureStatusAsync(await owner.Client.DeleteAsync($"/api/repository/{toDelete.Id}"),
            HttpStatusCode.NoContent);

        var branches = await GetListAsync<BranchDto>(owner.Client, $"/api/repository/{toKeep.Id}/branches");
        await Assert.That(branches.Select(x => x.Name)).IsEquivalentTo(new[] { "main" });
    }

    [Test]
    [Skip("Znany błąd. Każdy, także niezalogowany, może zrobić push do dowolnego repozytorium.")]
    public async Task Push_ByOutsiderWithoutCredentials_IsRejected()
    {
        using var git = new GitCli();
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var repository = await CreateRepositoryAsync(owner, project.Id);

        var work = await git.CloneAsync(repository.RepoUrl, "work");
        await git.RunAsync(work, "checkout", "-b", "main");
        await GitCli.WriteFileAsync(work, "obcy.txt", "nie powinno mnie tu być\n");
        await git.CommitAllAsync(work, "Obcy commit");
        var push = await git.TryRunAsync(work, "push", "origin", "main");

        await Assert.That(push.ExitCode).IsNotEqualTo(0);
        var branches = await GetListAsync<BranchDto>(owner.Client, $"/api/repository/{repository.Id}/branches");
        await Assert.That(branches.Count).IsEqualTo(0);
    }

    [Test]
    [Skip("Znany błąd. Klonowanie przez /api/git/... działa bez logowania, więc kod prywatnego projektu jest publiczny.")]
    public async Task InfoRefs_Anonymous_Returns401()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var repository = await CreateRepositoryAsync(owner, project.Id);

        var response = await Anonymous().GetAsync(
            $"/api/git/{project.Id}/{repository.Name}.git/info/refs?service=git-upload-pack");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }
}
