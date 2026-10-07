using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;
using Microsoft.AspNetCore.Http;
using ProjectCowork.Infrastructure.Integrations.Git.Abstractions;

namespace ProjectCowork.Infrastructure.Integrations.Git.Internal;

public class GitServerService : IGitServerService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GitServerService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> UploadAndCreateRemoteRepositoryAsync(string name, CancellationToken ct)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        using var process = new Process();

        var baseRepoFolder = Path.Combine(Directory.GetCurrentDirectory(), "Repos");
        var gitPath = Path.Combine(baseRepoFolder, $"{name}.git");
        
        var requestPath = httpContext.Request.Path.Value ?? "";
        var gitAction = requestPath.Contains(".git/") 
            ? requestPath.Substring(requestPath.IndexOf(".git/") + 5) 
            : "";
        
        if (!Directory.Exists(gitPath))
        {
            Directory.CreateDirectory(gitPath);
            await Process.Start("git", $"init --bare \"{gitPath}\"").WaitForExitAsync(ct);
            await Process.Start("git", $"-C \"{gitPath}\" config http.receivepack true").WaitForExitAsync(ct);
        }
        
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "http-backend --stateless-rpc --advertise-refs",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = gitPath,
            EnvironmentVariables =
            {
                { "GIT_HTTP_EXPORT_ALL", "1" },
                { "HTTP_GIT_PROTOCOL", httpContext.Request.Headers["Git-Protocol"] },
                { "REQUEST_METHOD", httpContext.Request.Method },
                { "GIT_PROJECT_ROOT", baseRepoFolder },
                { "PATH_INFO", $"/{name}.git/{gitAction}"},
                { "QUERY_STRING", httpContext.Request.QueryString.ToUriComponent().TrimStart('?') },
                { "CONTENT_TYPE", httpContext.Request.ContentType },
                { "CONTENT_LENGTH", httpContext.Request.ContentLength?.ToString() },
                { "HTTP_CONTENT_ENCODING", httpContext.Request.Headers["Content-Encoding"].ToString() },
                { "REMOTE_USER", httpContext.User.Identity?.Name },
                { "REMOTE_ADDR", httpContext.Connection.RemoteIpAddress?.ToString() },
                { "GIT_COMMITTER_NAME", httpContext.User.Identity?.Name },
            },
        };
        
        process.Start();
        
        var inputTask = Task.Run(async () => 
        {
            try 
            {
                await httpContext.Request.Body.CopyToAsync(process.StandardInput.BaseStream, ct);
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
 
            if (line.Length == 1)
                return (reader.Position, true);
 
            var colon = line.IndexOf((byte)':');
            if (colon == -1)
                break;
 
            var headerName = Encoding.UTF8.GetString(line[..colon]);
            var headerValue = Encoding.UTF8.GetString(line[(colon + 1)..]).Trim();
            httpContext.Response.Headers[headerName] = headerValue;
        }
 
        return (reader.Position, false);
    }
}