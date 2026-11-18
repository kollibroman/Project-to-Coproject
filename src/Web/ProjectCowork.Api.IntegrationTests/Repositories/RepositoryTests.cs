using System.Net;
using ProjectCowork.Api.IntegrationTests.Infrastructure;
using ProjectCowork.Core.Features.Models;
using ProjectCowork.Core.Features.Repositories.Models;

namespace ProjectCowork.Api.IntegrationTests.Repositories;

[ClassDataSource<ApiFactory>(Shared = SharedType.PerTestSession)]
public class RepositoryTests(ApiFactory factory) : ApiTestBase(factory)
{
    private string BareRepositoryPath(Guid projectId, string name) =>
        Path.Combine(Factory.ReposRoot, projectId.ToString(), $"{name}.git");

    private async Task<(TestUser Owner, ProjectModel Project, RepositoryDto Repository)> CreatePushedRepositoryAsync(
        GitCli git)
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var repository = await CreateRepositoryAsync(owner, project.Id);

        var work = await git.CloneAsync(repository.RepoUrl, "work");
        await git.RunAsync(work, "checkout", "-b", "main");

        await GitCli.WriteFileAsync(work, "README.md", "# Projekt\n");
        await git.CommitAllAsync(work, "Pierwszy commit");

        await GitCli.WriteFileAsync(work, "src/app.txt", "Witaj świecie\n");
        await File.WriteAllBytesAsync(Path.Combine(work, "logo.bin"), [0, 1, 2, 0, 255]);
        await git.CommitAllAsync(work, "Drugi commit");

        await git.RunAsync(work, "push", "origin", "main");

        return (owner, project, repository);
    }

    [Test]
    public async Task Create_ReturnsCloneUrl_CreatesBareRepository_AndListsIt()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var name = Unique("repo");

        var response = await PostRepositoryAsync(owner.Client, project.Id, name);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var cloneUrl = await response.Content.ReadFromJsonAsync<string>();
        await Assert.That(cloneUrl).IsEqualTo(new Uri(Factory.BaseAddress, $"/api/git/{project.Id}/{name}.git").ToString());

        var repositories = await GetListAsync<RepositoryDto>(owner.Client, $"/api/repository/project/{project.Id}");
        var listed = repositories.Single();
        await Assert.That(listed.Name).IsEqualTo(name);
        await Assert.That(listed.RepoUrl).IsEqualTo(cloneUrl);

        var single = await owner.Client.GetFromJsonAsync<RepositoryDto>($"/api/repository/{listed.Id}");
        await Assert.That(single!.Name).IsEqualTo(name);

        var bare = BareRepositoryPath(project.Id, name);
        await Assert.That(File.Exists(Path.Combine(bare, "HEAD"))).IsTrue();
        await Assert.That(await File.ReadAllTextAsync(Path.Combine(bare, "HEAD"))).IsEqualTo("ref: refs/heads/main\n");
    }

    [Test]
    public async Task Create_ByMember_IsAllowed()
    {
        var owner = await RegisterAsync();
        var member = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await AddMemberAsync(owner, project.Id, member, MemberRole);

        var response = await PostRepositoryAsync(member.Client, project.Id, Unique("repo"));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Create_ByObserver_Returns403_AndCreatesNothingOnDisk()
    {
        var owner = await RegisterAsync();
        var observer = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await AddMemberAsync(owner, project.Id, observer, ObserverRole);
        var name = Unique("repo");

        var response = await PostRepositoryAsync(observer.Client, project.Id, name);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        await Assert.That(Directory.Exists(BareRepositoryPath(project.Id, name))).IsFalse();
    }

    [Test]
    public async Task Create_Anonymous_Returns401()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await PostRepositoryAsync(Anonymous(), project.Id, Unique("repo"));

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    [Arguments("../ucieczka")]
    [Arguments("..")]
    [Arguments(".ukryte")]
    [Arguments("a/b")]
    public async Task Create_InvalidName_IsRejected_AndCreatesNothing(string name)
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await PostRepositoryAsync(owner.Client, project.Id, name);

        await Assert.That(response.IsSuccessStatusCode).IsFalse();
        var repositories = await GetListAsync<RepositoryDto>(owner.Client, $"/api/repository/project/{project.Id}");
        await Assert.That(repositories.Count).IsEqualTo(0);

        var anyGitDirOutsideRepos = Directory
            .EnumerateDirectories(Factory.WorkDirectory, "*.git", SearchOption.AllDirectories)
            .Any(path => !path.StartsWith(Factory.ReposRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal));
        await Assert.That(anyGitDirOutsideRepos).IsFalse();
    }

    [Test]
    public async Task Create_DuplicateName_IsRejected_AndListedOnce()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var repository = await CreateRepositoryAsync(owner, project.Id);

        var response = await PostRepositoryAsync(owner.Client, project.Id, repository.Name);

        await Assert.That(response.IsSuccessStatusCode).IsFalse();
        var repositories = await GetListAsync<RepositoryDto>(owner.Client, $"/api/repository/project/{project.Id}");
        await Assert.That(repositories.Count).IsEqualTo(1);
    }

    [Test]
    public async Task SameName_InTwoProjects_IsAllowed()
    {
        var owner = await RegisterAsync();
        var first = await CreateProjectAsync(owner);
        var second = await CreateProjectAsync(owner);
        var name = Unique("repo");

        await CreateRepositoryAsync(owner, first.Id, name);
        var response = await PostRepositoryAsync(owner.Client, second.Id, name);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task List_Outsider_Returns403_Anonymous_Returns401()
    {
        var owner = await RegisterAsync();
        var outsider = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var repository = await CreateRepositoryAsync(owner, project.Id);

        var list = await outsider.Client.GetAsync($"/api/repository/project/{project.Id}");
        var single = await outsider.Client.GetAsync($"/api/repository/{repository.Id}");
        var branches = await outsider.Client.GetAsync($"/api/repository/{repository.Id}/branches");
        var anonymous = await Anonymous().GetAsync($"/api/repository/{repository.Id}/branches");

        await Assert.That(list.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        await Assert.That(single.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        await Assert.That(branches.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        await Assert.That(anonymous.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Observer_CanBrowse()
    {
        var owner = await RegisterAsync();
        var observer = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await AddMemberAsync(owner, project.Id, observer, ObserverRole);
        var repository = await CreateRepositoryAsync(owner, project.Id);

        var response = await observer.Client.GetAsync($"/api/repository/{repository.Id}/branches");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task UnknownRepository_Returns404()
    {
        var user = await RegisterAsync();

        var response = await user.Client.GetAsync($"/api/repository/{Guid.NewGuid()}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task EmptyRepository_HasNoBranches_AndTreeReturns404()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var repository = await CreateRepositoryAsync(owner, project.Id);

        var branches = await GetListAsync<BranchDto>(owner.Client, $"/api/repository/{repository.Id}/branches");
        var tree = await owner.Client.GetAsync($"/api/repository/{repository.Id}/tree?branch=main");

        await Assert.That(branches.Count).IsEqualTo(0);
        await Assert.That(tree.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Push_ThenBranchesShowMainAsDefault()
    {
        using var git = new GitCli();
        var (owner, _, repository) = await CreatePushedRepositoryAsync(git);

        var branches = await GetListAsync<BranchDto>(owner.Client, $"/api/repository/{repository.Id}/branches");

        await Assert.That(branches.Count).IsEqualTo(1);
        await Assert.That(branches[0].Name).IsEqualTo("main");
        await Assert.That(branches[0].IsDefault).IsTrue();
    }

    [Test]
    public async Task Tree_Root_ListsFoldersFirst_ThenFilesWithSizes()
    {
        using var git = new GitCli();
        var (owner, _, repository) = await CreatePushedRepositoryAsync(git);

        var tree = await GetListAsync<TreeEntryDto>(owner.Client, $"/api/repository/{repository.Id}/tree?branch=main");

        await Assert.That(tree.Select(x => x.Name)).IsEquivalentTo(new[] { "src", "logo.bin", "README.md" },
            TUnit.Assertions.Enums.CollectionOrdering.Matching);
        await Assert.That(tree[0].Type).IsEqualTo(TreeEntryType.Folder);
        await Assert.That(tree[0].Size).IsNull();
        await Assert.That(tree.Single(x => x.Name == "README.md").Size).IsEqualTo(10L);
    }

    [Test]
    public async Task Tree_Subfolder_ReturnsFullPaths()
    {
        using var git = new GitCli();
        var (owner, _, repository) = await CreatePushedRepositoryAsync(git);

        var tree = await GetListAsync<TreeEntryDto>(owner.Client,
            $"/api/repository/{repository.Id}/tree?branch=main&path=src");

        await Assert.That(tree.Count).IsEqualTo(1);
        await Assert.That(tree[0].Path).IsEqualTo("src/app.txt");
        await Assert.That(tree[0].Type).IsEqualTo(TreeEntryType.File);
    }

    [Test]
    public async Task File_Text_ReturnsUtf8Content()
    {
        using var git = new GitCli();
        var (owner, _, repository) = await CreatePushedRepositoryAsync(git);

        var file = await owner.Client.GetFromJsonAsync<FileContentDto>(
            $"/api/repository/{repository.Id}/file?branch=main&path=src/app.txt");

        await Assert.That(file!.Content).IsEqualTo("Witaj świecie\n");
        await Assert.That(file.IsBinary).IsFalse();
    }

    [Test]
    public async Task File_Binary_HasNoContent()
    {
        using var git = new GitCli();
        var (owner, _, repository) = await CreatePushedRepositoryAsync(git);

        var file = await owner.Client.GetFromJsonAsync<FileContentDto>(
            $"/api/repository/{repository.Id}/file?branch=main&path=logo.bin");

        await Assert.That(file!.IsBinary).IsTrue();
        await Assert.That(file.Content).IsNull();
        await Assert.That(file.Size).IsEqualTo(5L);
    }

    [Test]
    [Arguments("tree?branch=nie-ma")]
    [Arguments("tree?branch=main&path=nie-ma")]
    [Arguments("file?branch=main&path=nie-ma.txt")]
    [Arguments("file?branch=main&path=src")]
    [Arguments("commits?branch=nie-ma")]
    public async Task Browse_MissingBranchOrPath_Returns404(string query)
    {
        using var git = new GitCli();
        var (owner, _, repository) = await CreatePushedRepositoryAsync(git);

        var response = await owner.Client.GetAsync($"/api/repository/{repository.Id}/{query}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Commits_NewestFirst_WithPaging()
    {
        using var git = new GitCli();
        var (owner, _, repository) = await CreatePushedRepositoryAsync(git);

        var all = await owner.Client.GetFromJsonAsync<CommitsPageDto>(
            $"/api/repository/{repository.Id}/commits?branch=main");
        var firstPage = await owner.Client.GetFromJsonAsync<CommitsPageDto>(
            $"/api/repository/{repository.Id}/commits?branch=main&page=1&pageSize=1");
        var secondPage = await owner.Client.GetFromJsonAsync<CommitsPageDto>(
            $"/api/repository/{repository.Id}/commits?branch=main&page=2&pageSize=1");

        await Assert.That(all!.Items.Select(x => x.Message))
            .IsEquivalentTo(["Drugi commit", "Pierwszy commit"], TUnit.Assertions.Enums.CollectionOrdering.Matching);
        await Assert.That(all.HasMore).IsFalse();
        await Assert.That(all.Items[0].AuthorName).IsEqualTo("Tester");
        await Assert.That(all.Items[0].AuthorEmail).IsEqualTo("tester@example.com");
        await Assert.That(all.Items[0].Sha.Length).IsEqualTo(40);

        await Assert.That(firstPage!.Items.Single().Message).IsEqualTo("Drugi commit");
        await Assert.That(firstPage.HasMore).IsTrue();
        await Assert.That(secondPage!.Items.Single().Message).IsEqualTo("Pierwszy commit");
        await Assert.That(secondPage.HasMore).IsFalse();
    }

    [Test]
    public async Task Commits_HeadMatchesLocalHead()
    {
        using var git = new GitCli();
        var (owner, _, repository) = await CreatePushedRepositoryAsync(git);
        var localHead = (await git.RunAsync(git.PathTo("work"), "rev-parse", "HEAD")).Trim();

        var commits = await owner.Client.GetFromJsonAsync<CommitsPageDto>(
            $"/api/repository/{repository.Id}/commits?branch=main&pageSize=1");

        await Assert.That(commits!.Items[0].Sha).IsEqualTo(localHead);
    }

    [Test]
    public async Task Clone_AfterPush_ReturnsSameContent()
    {
        using var git = new GitCli();
        var (_, _, repository) = await CreatePushedRepositoryAsync(git);

        var copy = await git.CloneAsync(repository.RepoUrl, "copy");

        await Assert.That(await File.ReadAllTextAsync(Path.Combine(copy, "src", "app.txt"))).IsEqualTo("Witaj świecie\n");
        await Assert.That(await File.ReadAllBytesAsync(Path.Combine(copy, "logo.bin")))
            .IsEquivalentTo(new byte[] { 0, 1, 2, 0, 255 });
    }

    [Test]
    public async Task PushFeatureBranch_ShowsUpBesideDefault()
    {
        using var git = new GitCli();
        var (owner, _, repository) = await CreatePushedRepositoryAsync(git);
        var work = git.PathTo("work");

        await git.RunAsync(work, "checkout", "-b", "feature/logowanie");
        await GitCli.WriteFileAsync(work, "login.txt", "logowanie\n");
        await git.CommitAllAsync(work, "Logowanie");
        await git.RunAsync(work, "push", "origin", "feature/logowanie");

        var branches = await GetListAsync<BranchDto>(owner.Client, $"/api/repository/{repository.Id}/branches");
        var featureTree = await GetListAsync<TreeEntryDto>(owner.Client,
            $"/api/repository/{repository.Id}/tree?branch=feature/logowanie");
        var mainTree = await GetListAsync<TreeEntryDto>(owner.Client,
            $"/api/repository/{repository.Id}/tree?branch=main");

        await Assert.That(branches.Select(x => x.Name)).IsEquivalentTo(new[] { "main", "feature/logowanie" },
            TUnit.Assertions.Enums.CollectionOrdering.Matching);
        await Assert.That(branches.Single(x => x.IsDefault).Name).IsEqualTo("main");
        await Assert.That(featureTree.Any(x => x.Name == "login.txt")).IsTrue();
        await Assert.That(mainTree.Any(x => x.Name == "login.txt")).IsFalse();
    }

    [Test]
    public async Task GitEndpoint_UnknownRepository_Returns404()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);

        var response = await owner.Client.GetAsync(
            $"/api/git/{project.Id}/nie-ma.git/info/refs?service=git-upload-pack");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Delete_ByOwner_RemovesFromListAndDisk()
    {
        var owner = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        var repository = await CreateRepositoryAsync(owner, project.Id);

        var response = await owner.Client.DeleteAsync($"/api/repository/{repository.Id}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
        var repositories = await GetListAsync<RepositoryDto>(owner.Client, $"/api/repository/project/{project.Id}");
        await Assert.That(repositories.Count).IsEqualTo(0);
        await Assert.That(Directory.Exists(BareRepositoryPath(project.Id, repository.Name))).IsFalse();
    }

    [Test]
    [Arguments(MemberRole)]
    [Arguments(ObserverRole)]
    public async Task Delete_WithoutRepositoriesDelete_Returns403_AndKeepsRepository(string role)
    {
        var owner = await RegisterAsync();
        var member = await RegisterAsync();
        var project = await CreateProjectAsync(owner);
        await AddMemberAsync(owner, project.Id, member, role);
        var repository = await CreateRepositoryAsync(owner, project.Id);

        var response = await member.Client.DeleteAsync($"/api/repository/{repository.Id}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        await Assert.That(Directory.Exists(BareRepositoryPath(project.Id, repository.Name))).IsTrue();
    }

    [Test]
    public async Task Delete_Unknown_Returns404()
    {
        var user = await RegisterAsync();

        var response = await user.Client.DeleteAsync($"/api/repository/{Guid.NewGuid()}");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }
}
