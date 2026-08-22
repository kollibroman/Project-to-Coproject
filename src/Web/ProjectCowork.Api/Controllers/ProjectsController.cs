using DispatchR;
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
        group.MapGet("/projects/{projectId}", GetProjectAsync);
        group.MapDelete("/api/projects/{projectId}", DeleteProjectAsync);
        group.MapPost("/api/projects", CreateProjectAsync);
        group.MapPost("/api/projects/{projectId}/edit", EditProjectAsync);
        
        return endpoints;
    }

    private static IAsyncEnumerable<ProjectModel> GetProjectsAsync([FromServices] IMediator mediator, CancellationToken ct)
    {
        return mediator.CreateStream(new GetProjectsQuery(), ct);
    }

    private static async Task<ProjectModel> GetProjectAsync([FromRoute] Guid projectId, [FromServices] IMediator mediator, CancellationToken ct)
    {
        return await mediator.Send(new GetProjectQuery
        {
            ProjectId = projectId
        }, ct);
    }

    private static async Task CreateProjectAsync([FromBody] CreateProjectRequest request, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new AddProjectCommand
        {
            Name = request.Name,
            Description = request.Description
        }, ct);
    }
    
    private static async Task EditProjectAsync([FromBody] EditProjectRequest request, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new EditProjectCommand
        {
            ProjectId =  request.ProjectId,
            Name = request.Name,
            Description = request.Description
        }, ct);
    }
    
    private static async Task DeleteProjectAsync([FromRoute] Guid projectId, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new DeleteProjectCommand
        {
            ProjectId = projectId
        }, ct);
    }
}