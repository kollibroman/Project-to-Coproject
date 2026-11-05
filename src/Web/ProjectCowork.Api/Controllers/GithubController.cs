using DispatchR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProjectCowork.Api.Models;
using ProjectCowork.Core.Features.Integrations.Github.Commands;
using ProjectCowork.Core.Features.Integrations.Github.Models;
using ProjectCowork.Core.Features.Integrations.Github.Queries;

namespace ProjectCowork.Api.Controllers;

public static class GithubController
{
    public static IEndpointRouteBuilder MapGithubControllerRoutes(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/github");

        group.MapGet("/status", GetConnectionStatusAsync);
        group.MapPost("/authorize", AuthenticateUserWithPatAsync);
        group.MapGet("/user/repositories", FetchUserRepositoriesAsync);
        group.MapPost("/repository/import", ImportRepositoryAsync);
        
        return endpoints;
    }

    private static async Task<Ok<GithubConnectionStatusDto>> GetConnectionStatusAsync([FromServices] IMediator mediator,
        CancellationToken ct)
    {
        var status = await mediator.Send(new GithubConnectionStatusQuery(), ct);

        return TypedResults.Ok(status);
    }

    private static async Task<Ok> AuthenticateUserWithPatAsync([FromBody] GithubAuthorizePatRequest request,
        [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new GithubAuthorizeWithPatCommand
        {
            Pat = request.Pat,
            GithubUsername = request.Login
        }, ct);
        
        return TypedResults.Ok();
    }

    private static Ok<IAsyncEnumerable<UserRepositoryDto>> FetchUserRepositoriesAsync([FromServices] IMediator mediator, CancellationToken ct)
    {
        var result = mediator.CreateStream(new GithubUserRepositoriesQuery(), ct);
        
        return TypedResults.Ok(result);
    }

    private static async Task<Ok> ImportRepositoryAsync([FromBody] ImportGithubRepositoryRequest request, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new GithubImportProjectCommand
        {
            ProjectId = request.ProjectId,
            CloneUrl = request.CloneUrl,
            RepositoryName = request.RepositoryName
        }, ct);
        
        return TypedResults.Ok();
    }
}