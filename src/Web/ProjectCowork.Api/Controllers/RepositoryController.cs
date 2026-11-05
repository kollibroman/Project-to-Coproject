using DispatchR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProjectCowork.Api.Models;
using ProjectCowork.Core.Features.Repositories.Commands;
using ProjectCowork.Core.Features.Repositories.Models;
using ProjectCowork.Core.Features.Repositories.Queries;

namespace ProjectCowork.Api.Controllers;

public static class RepositoryController
{
    public static IEndpointRouteBuilder MapRepositories(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/repository");

        group.MapGet("/project/{projectId:guid}", GetRepositoriesAsync);
        group.MapGet("/{repositoryId:guid}", GetRepositoryAsync);
        group.MapDelete("/{repositoryId:guid}", DeleteRepositoryAsync);

        // Przeglądanie zawartości. Gałąź i ścieżka idą w query, bo mogą zawierać "/".
        group.MapGet("/{repositoryId:guid}/branches", GetBranchesAsync);
        group.MapGet("/{repositoryId:guid}/tree", GetTreeAsync);
        group.MapGet("/{repositoryId:guid}/file", GetFileAsync);
        group.MapGet("/{repositoryId:guid}/commits", GetCommitsAsync);
        
        return endpoints;
    }

    private static async Task<Ok<IEnumerable<BranchDto>>> GetBranchesAsync([FromRoute] Guid repositoryId,
        [FromServices] IMediator mediator, CancellationToken ct)
    {
        var branches = await mediator.Send(new GetRepositoryBranchesQuery
        {
            RepositoryId = repositoryId
        }, ct);

        return TypedResults.Ok(branches);
    }

    private static async Task<Ok<IEnumerable<TreeEntryDto>>> GetTreeAsync([FromRoute] Guid repositoryId,
        [FromQuery] string branch, [FromQuery] string? path, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var tree = await mediator.Send(new GetRepositoryTreeQuery
        {
            RepositoryId = repositoryId,
            Branch = branch,
            Path = path ?? string.Empty
        }, ct);

        return TypedResults.Ok(tree);
    }

    private static async Task<Ok<FileContentDto>> GetFileAsync([FromRoute] Guid repositoryId,
        [FromQuery] string branch, [FromQuery] string path, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var file = await mediator.Send(new GetRepositoryFileQuery
        {
            RepositoryId = repositoryId,
            Branch = branch,
            Path = path
        }, ct);

        return TypedResults.Ok(file);
    }

    private static async Task<Ok<CommitsPageDto>> GetCommitsAsync([FromRoute] Guid repositoryId,
        [FromQuery] string branch, [FromServices] IMediator mediator, CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int pageSize = GetRepositoryCommitsQuery.DefaultPageSize)
    {
        var commits = await mediator.Send(new GetRepositoryCommitsQuery
        {
            RepositoryId = repositoryId,
            Branch = branch,
            Page = page,
            PageSize = pageSize
        }, ct);

        return TypedResults.Ok(commits);
    }

    private static async Task<Ok<IEnumerable<RepositoryDto>>> GetRepositoriesAsync([FromRoute] Guid projectId, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var repositories = await mediator.Send(new GetRepositoriesQuery
        {
            ProjectId = projectId
        }, ct);

        return TypedResults.Ok(repositories);
    }

    private static async Task<Ok<RepositoryDto>> GetRepositoryAsync([FromRoute] Guid repositoryId, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var repository = await mediator.Send(new GetRepositoryQuery
        {
            RepositoryId = repositoryId
        }, ct);

        return TypedResults.Ok(repository);
    }

    private static async Task<NoContent> DeleteRepositoryAsync([FromRoute] Guid repositoryId, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new DeleteRepositoryCommand
        {
            RepositoryId = repositoryId
        }, ct);

        return TypedResults.NoContent();
    }
}
