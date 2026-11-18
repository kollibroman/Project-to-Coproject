using System.Diagnostics;

namespace ProjectCowork.Api.IntegrationTests.Infrastructure;

public sealed class GitCli : IDisposable
{
    private string Root { get; } = Path.Combine(Path.GetTempPath(), $"projectcowork-git-{Guid.NewGuid():N}");

    public GitCli()
    {
        Directory.CreateDirectory(Root);
    }

    public string PathTo(string name) => Path.Combine(Root, name);

    public async Task<string> RunAsync(string workingDirectory, params string[] args)
    {
        var result = await TryRunAsync(workingDirectory, args);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"git {string.Join(' ', args)} exited with {result.ExitCode}.\n{result.Output}\n{result.Error}");
        }

        return result.Output;
    }

    public async Task<(int ExitCode, string Output, string Error)> TryRunAsync(string workingDirectory,
        params string[] args)
    {
        var startInfo = new ProcessStartInfo("git")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var arg in new[]
                 {
                     "-c", "user.name=Tester", "-c", "user.email=tester@example.com",
                     "-c", "init.defaultBranch=main", "-c", "commit.gpgsign=false"
                 }.Concat(args))
        {
            startInfo.ArgumentList.Add(arg);
        }

        startInfo.Environment["GIT_CONFIG_NOSYSTEM"] = "1";
        startInfo.Environment["GIT_CONFIG_GLOBAL"] = "/dev/null";
        startInfo.Environment["GIT_TERMINAL_PROMPT"] = "0";

        using var process = Process.Start(startInfo)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        await process.WaitForExitAsync(timeout.Token);

        return (process.ExitCode, await output, await error);
    }

    public async Task<string> CloneAsync(string url, string directoryName)
    {
        await RunAsync(Root, "clone", url, directoryName);
        return PathTo(directoryName);
    }

    public static async Task WriteFileAsync(string repositoryDirectory, string relativePath, string content)
    {
        var path = Path.Combine(repositoryDirectory, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, content);
    }

    public async Task CommitAllAsync(string repositoryDirectory, string message)
    {
        await RunAsync(repositoryDirectory, "add", "--all");
        await RunAsync(repositoryDirectory, "commit", "-m", message);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(Root, recursive: true);
        }
        catch (IOException)
        {
            // Pliki obiektów git bywają tylko do odczytu, a sprzątanie nie może psuć testu.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
