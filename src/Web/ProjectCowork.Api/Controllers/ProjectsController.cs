using DispatchR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProjectCowork.Api.Models;
using ProjectCowork.Core.Features.Commands;
using ProjectCowork.Core.Features.Models;
using ProjectCowork.Core.Features.Queries;

namespace ProjectCowork.Api.Controllers;

public static class ProjectsController
{
    public static IEndpointRouteBuilder AddProjects(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/projects");

        group.MapGet("/", GetProjectsAsync);
        group.MapGet("/discover", GetDiscoverProjectsAsync);
        group.MapGet("/{projectId}", GetProjectAsync);
        group.MapDelete("/{projectId}", DeleteProjectAsync);
        group.MapPost("/", CreateProjectAsync);
        group.MapPost("/{projectId}/edit", EditProjectAsync);
        group.MapPost("/{projectId}/invite", InviteUserAsync);
        
        return endpoints;
    }

    private static Ok<IAsyncEnumerable<ProjectModel>> GetProjectsAsync([FromServices] IMediator mediator, CancellationToken ct)
    {
        var stream = mediator.CreateStream(new GetProjectsQuery(), ct);
        return TypedResults.Ok(stream);
    }

    private static Ok<IAsyncEnumerable<ProjectModel>> GetDiscoverProjectsAsync([FromServices] IMediator mediator, CancellationToken ct)
    {
        var stream = mediator.CreateStream(new GetDiscoverProjectsQuery(), ct);
        return TypedResults.Ok(stream);
    }

    private static async Task<Ok<ProjectModel>> GetProjectAsync([FromRoute] Guid projectId, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var project = await mediator.Send(new GetProjectQuery
        {
            ProjectId = projectId
        }, ct);
        
        return TypedResults.Ok(project);
    }

    private static async Task<Created> CreateProjectAsync([FromBody] CreateProjectRequest request, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new AddProjectCommand
        {
            Name = request.Name,
            Description = request.Description
        }, ct);

        return TypedResults.Created();
    }
    
    private static async Task<NoContent> EditProjectAsync([FromBody] EditProjectRequest request, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new EditProjectCommand
        {
            ProjectId =  request.ProjectId,
            Name = request.Name,
            Description = request.Description
        }, ct);

        return TypedResults.NoContent();
    }
    
    private static async Task<NoContent> InviteUserAsync([FromRoute] Guid projectId, [FromBody] InviteUserRequest request, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new InviteUserToProjectCommand
        {
            ProjectId = projectId,
            Email = request.Email
        }, ct);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteProjectAsync([FromRoute] Guid projectId, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new DeleteProjectCommand
        {
            ProjectId = projectId
        }, ct);

        return TypedResults.NoContent();
    }
}