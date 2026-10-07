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
        endpoints.MapGroup("/api/git");
        
        endpoints.MapPost("/repository/create", CreateRepositoryAsync);
        endpoints.Map("/{projectId:guid}/{name}.git/{*path}", PushToRepositoryAsync);
        
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

    private static async Task PushToRepositoryAsync([FromServices] IMediator mediator, [FromRoute] Guid projectId, [FromRoute] string name, CancellationToken ct)
    {
        await mediator.Send(new PushToRepositoryCommand
        {
            Name = name
        }, ct);
    }
}