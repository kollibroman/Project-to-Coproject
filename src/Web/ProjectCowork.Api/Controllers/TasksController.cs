using DispatchR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProjectCowork.Api.Models;
using ProjectCowork.Core.Features.Management.Commands;
using ProjectCowork.Core.Features.Management.Models;
using ProjectCowork.Core.Features.Management.Queries;

namespace ProjectCowork.Api.Controllers;

public static class TasksController
{
    public static IEndpointRouteBuilder MapTasks(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tasks");

        group.MapGet("/project/{projectId:Guid}", GetProjectTasksAsync);
        group.MapGet("/{taskId:Guid}", GetTaskAsync);
        group.MapPost("/", AddTaskAsync);
        group.MapPut("/{taskId:Guid}", EditTaskAsync);
        group.MapDelete("/{taskId:Guid}", DeleteTaskAsync);

        return endpoints;
    }

    private static Ok<IAsyncEnumerable<TaskDto>> GetProjectTasksAsync([FromRoute] Guid projectId, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var result = mediator.CreateStream(new GetTasksFromProjectQuery
        {
            ProjectId = projectId
        }, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<TaskDto>> GetTaskAsync([FromRoute] Guid taskId, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTaskQuery
        {
            TaskId = taskId
        }, ct);

        return TypedResults.Ok(result);
    }

    private static async Task<Created> AddTaskAsync([FromBody] AddTaskRequest request, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new AddTaskCommand
        {
            ProjectId = request.ProjectId,
            Name = request.Name,
            Description = request.Description,
            Priority = request.Priority,
            Status = request.Status,
            SprintId = request.SprintId,
            AssignedUserId = request.AssignedUserId
        }, ct);

        return TypedResults.Created();
    }

    private static async Task<NoContent> EditTaskAsync([FromRoute] Guid taskId, [FromBody] EditTaskRequest request, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new EditTaskCommand
        {
            TaskId = taskId,
            Name = request.Name,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            SprintId = request.SprintId,
            AssignedUserId = request.AssignedUserId
        }, ct);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteTaskAsync([FromRoute] Guid taskId, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new DeleteTaskCommand
        {
            TaskId = taskId
        }, ct);

        return TypedResults.NoContent();
    }
}
