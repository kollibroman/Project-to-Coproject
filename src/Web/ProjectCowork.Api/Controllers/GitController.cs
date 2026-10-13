using DispatchR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProjectCowork.Api.Models;
using ProjectCowork.Core.Features.Repositories.Commands;

namespace ProjectCowork.Api.Controllers;

public static class GitController
{
    public static IEndpointRouteBuilder MapGitEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/git");
        
        group.MapPost("/repository/create", CreateRepositoryAsync);
        
        group.MapMethods("/{repositoryId:guid}/{name}.git/{*path}", [HttpMethods.Get, HttpMethods.Post], PushToRepositoryAsync);
        
        return endpoints;     
    }

    private static async Task<Ok<string>> CreateRepositoryAsync([FromServices] IMediator mediator, [FromBody] CreateRepositoryRequest request, CancellationToken ct)
    {
        var clonePath = await mediator.Send(new CreateRepositoryCommand
        {
            Name = request.Name,
            ProjectId = request.ProjectId
        }, ct);
        
        return TypedResults.Ok(clonePath);
    }

    private static async Task PushToRepositoryAsync([FromServices] IMediator mediator, [FromRoute] Guid repositoryId, [FromRoute] string name, [FromRoute] string? path, CancellationToken ct)
    {
        await mediator.Send(new PushToRepositoryCommand
        {
            RepositoryId = repositoryId,
            Name = name,
            GitAction = path ?? string.Empty
        }, ct);
    }
}