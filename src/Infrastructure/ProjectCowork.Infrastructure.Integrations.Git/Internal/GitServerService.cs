using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProjectCowork.Infrastructure.Integrations.Git.Abstractions;

namespace ProjectCowork.Infrastructure.Integrations.Git.Internal;

public class GitServerService : IGitServerService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<GitServerService> _logger;

    public GitServerService(IHttpContextAccessor httpContextAccessor, ILogger<GitServerService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<string> UploadAndCreateRemoteRepositoryAsync(Guid projectId, string name, string gitAction, CancellationToken ct)
    {
        var httpContext = _httpContextAccessor.HttpContext
                          ?? throw new InvalidOperationException("No active HTTP context.");

        // Repos/{projectId}/{name}.git - same layout as GitClientService.
        var projectRoot = GitRepositoryPaths.GetProjectRoot(projectId);
        var gitPath = GitRepositoryPaths.IsValidName(name)
            ? GitRepositoryPaths.GetRepositoryPath(projectId, name)
            : null;

        if (gitPath is null || !Directory.Exists(gitPath))
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return string.Empty;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "http-backend",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = gitPath,
        };

        void SetEnv(string key, string? value)
        {
            if (!string.IsNullOrEmpty(value))
                startInfo.Environment[key] = value;
        }

        var request = httpContext.Request;
        SetEnv("GIT_HTTP_EXPORT_ALL", "1");
        SetEnv("GIT_PROJECT_ROOT", projectRoot);
        SetEnv("PATH_INFO", $"/{name}.git/{gitAction}");
        SetEnv("REQUEST_METHOD", request.Method);
        SetEnv("QUERY_STRING", request.QueryString.ToUriComponent().TrimStart('?'));
        SetEnv("CONTENT_TYPE", request.ContentType);
        SetEnv("CONTENT_LENGTH", request.ContentLength?.ToString());
        SetEnv("HTTP_CONTENT_ENCODING", request.Headers["Content-Encoding"].ToString());
        SetEnv("GIT_PROTOCOL", request.Headers["Git-Protocol"].ToString());
        SetEnv("REMOTE_USER", httpContext.User.Identity?.Name);
        SetEnv("REMOTE_ADDR", httpContext.Connection.RemoteIpAddress?.ToString());

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stderrTask = process.StandardError.ReadToEndAsync(ct);

        var inputTask = Task.Run(async () =>
        {
            try
            {
                await request.Body.CopyToAsync(process.StandardInput.BaseStream, ct);
            }
            finally
            {
                process.StandardInput.Close();
            }
        }, ct);

        var pipeReader = PipeReader.Create(process.StandardOutput.BaseStream);
        await ReadResponse(pipeReader, httpContext, ct);
        await pipeReader.CopyToAsync(httpContext.Response.Body, ct);
        await pipeReader.CompleteAsync();

        await inputTask;
        await process.WaitForExitAsync(ct);

        var stderr = await stderrTask;
        if (process.ExitCode != 0 || !string.IsNullOrWhiteSpace(stderr))
        {
            _logger.LogWarning("git http-backend exited with {ExitCode} for {GitPath}: {Stderr}",
                process.ExitCode, gitPath, stderr);
        }

        return gitPath;
    }

    private static async Task ReadResponse(PipeReader pipeReader, HttpContext httpContext, CancellationToken cancellationToken)
    {
        while (true)
        {
            var result = await pipeReader.ReadAsync(cancellationToken);
            var buffer = result.Buffer;
            var (position, isFinished) = ReadHeaders(httpContext, buffer);
            pipeReader.AdvanceTo(position, buffer.End);

            if (result.IsCompleted || isFinished)
                break;
        }
    }

    private static (SequencePosition Position, bool IsFinished) ReadHeaders(
        HttpContext httpContext,
        in ReadOnlySequence<byte> sequence)
    {
        var reader = new SequenceReader<byte>(sequence);
        while (!reader.End)
        {
            if (!reader.TryReadTo(out ReadOnlySpan<byte> line, (byte)'\n'))
                break;

            // Empty line ("\r" or "") terminates the CGI header block.
            if (line.Length == 0 || (line.Length == 1 && line[0] == (byte)'\r'))
                return (reader.Position, true);

            var colon = line.IndexOf((byte)':');
            if (colon == -1)
                break;

            var headerName = Encoding.UTF8.GetString(line[..colon]);
            var headerValue = Encoding.UTF8.GetString(line[(colon + 1)..]).Trim();

            // CGI "Status: 404 Not Found" must become the HTTP status code, not a header.
            if (headerName.Equals("Status", StringComparison.OrdinalIgnoreCase))
            {
                var code = headerValue.Split(' ', 2)[0];
                if (int.TryParse(code, out var statusCode))
                    httpContext.Response.StatusCode = statusCode;
                continue;
            }

            httpContext.Response.Headers[headerName] = headerValue;
        }

        return (reader.Position, false);
    }
}
