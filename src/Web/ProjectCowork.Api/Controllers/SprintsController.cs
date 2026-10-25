using DispatchR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProjectCowork.Api.Models;
using ProjectCowork.Core.Features.Management.Commands;
using ProjectCowork.Core.Features.Management.Models;
using ProjectCowork.Core.Features.Management.Queries;

namespace ProjectCowork.Api.Controllers;

public static class SprintsController
{
    public static IEndpointRouteBuilder MapSprints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/sprints");

        group.MapGet("/project/{projectId:Guid}", GetProjectSprintsAsync);
        group.MapGet("/{sprintId:Guid}", GetSprintAsync);
        group.MapGet("/{sprintId:Guid}/tasks", GetSprintTasksAsync);
        group.MapPost("/", AddSprintAsync);
        group.MapPut("/{sprintId:Guid}", EditSprintAsync);
        group.MapDelete("/{sprintId:Guid}", DeleteSprintAsync);

        return endpoints;
    }

    private static Ok<IAsyncEnumerable<SprintDto>> GetProjectSprintsAsync([FromRoute] Guid projectId, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var result = mediator.CreateStream(new GetProjectSprintsQuery
        {
            ProjectId = projectId
        }, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<SprintDto>> GetSprintAsync([FromRoute] Guid sprintId, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSprintQuery
        {
            SprintId = sprintId
        }, ct);

        return TypedResults.Ok(result);
    }

    private static Ok<IAsyncEnumerable<TaskDto>> GetSprintTasksAsync([FromRoute] Guid sprintId, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var result = mediator.CreateStream(new GetTasksFromSprintQuery
        {
            SprintId = sprintId
        }, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Created> AddSprintAsync([FromBody] AddSprintRequest request, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new AddSprintCommand
        {
            ProjectId = request.ProjectId,
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        }, ct);

        return TypedResults.Created();
    }

    private static async Task<NoContent> EditSprintAsync([FromRoute] Guid sprintId, [FromBody] EditSprintRequest request, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new EditSprintCommand
        {
            SprintId = sprintId,
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        }, ct);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteSprintAsync([FromRoute] Guid sprintId, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new DeleteSprintCommand
        {
            SprintId = sprintId
        }, ct);

        return TypedResults.NoContent();
    }
}
