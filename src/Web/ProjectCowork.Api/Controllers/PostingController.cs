using DispatchR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProjectCowork.Api.Models;
using ProjectCowork.Core.Features.ProjectPosting.Commands;
using ProjectCowork.Core.Features.ProjectPosting.Models;
using ProjectCowork.Core.Features.ProjectPosting.Queries;

namespace ProjectCowork.Api.Controllers;

public static class PostingController
{
    public static IEndpointRouteBuilder MapPostings(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/postings");

        group.MapGet("/{projectId:Guid}", GetProjectPostingsAsync);
        group.MapGet("/posting/{projectPostingId:Guid}", GetProjectPostingAsync);
        group.MapPost("/posting/add", AddProjectPostingAsync);
        group.MapPut("/posting/update", UpdateProjectPostingAsync);
        group.MapPost("/posting/apply", ApplyForProjectPostingAsync);
        group.MapPost("/posting/{projectPostingId:Guid}/applications", GetPostingApplicationsAsync);
        
        return endpoints;
    }

    private static Ok<IAsyncEnumerable<ProjectPostingDto>> GetProjectPostingsAsync([FromRoute] Guid projectId, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var result = mediator.CreateStream(new GetProjectPostingsQuery
        {
            ProjectId = projectId
        }, ct);
        
        return TypedResults.Ok(result);
    }

    private static async Task<NoContent> AddProjectPostingAsync([FromBody] AddPostingRequest request, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new AddProjectPostingCommand
        {
            ProjectId = request.ProjectId,
            JobDescription = request.JobDescription,
            ProjectDescription = request.ProjectDescription,
        }, ct);
        
        return TypedResults.NoContent();
    }
    private static async Task<NoContent> UpdateProjectPostingAsync([FromBody] UpdatePostingRequest request, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new UpdateProjectPostingCommand
        {
            JobDescription = request.JobDescription,
            ProjectDescription = request.ProjectDescription,
            ProjectPostingId = request.ProjectPostingId,
            IsActive = request.IsActive,
        }, ct);
        
        return TypedResults.NoContent();
    }
    
    
    private static async Task<Ok<ProjectPostingDto>> GetProjectPostingAsync([FromRoute] Guid projectPostingId, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProjectPostingQuery
        {
            ProjectPostingId = projectPostingId
        }, ct);
        
        return TypedResults.Ok(result);
    }

    private static Ok<IAsyncEnumerable<ProjectApplicationDto>> GetPostingApplicationsAsync([FromRoute] Guid projectPostingId, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var result = mediator.CreateStream(new GetPostingApplicationsQuery
        {
            ProjectPostingId = projectPostingId
        }, ct);
        
        return TypedResults.Ok(result);
    }

    private static async Task<NoContent> ApplyForProjectPostingAsync([FromBody] ApplyForProjectRequest request, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var applicationCommand = new ApplyForPostingCommand
        {
            ProjectPostingId = request.ProjectPostingId,
            Description = request.Description,
            Attachments = request.Attachments.Select(x => new ApplyForPostingCommand.AttachmentModel
            {
                FileName = x.FileName,
                SizeInBytes = x.Length,
                ContentStream = x.OpenReadStream(),
                ContentType = x.ContentType,
            })
        };
        
        await mediator.Send(applicationCommand, ct);
        
        return TypedResults.NoContent();
    }
}