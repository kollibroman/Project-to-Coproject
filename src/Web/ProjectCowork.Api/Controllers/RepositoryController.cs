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
        
        return endpoints;
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
