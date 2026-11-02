using DispatchR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProjectCowork.Api.Models;
using ProjectCowork.Core.Features.Permissions.Commands;
using ProjectCowork.Core.Features.Permissions.Models;
using ProjectCowork.Core.Features.Permissions.Queries;

namespace ProjectCowork.Api.Controllers;

/// <summary>
/// Role i członkowie projektu. Uprawnienia sprawdzają handlery (IPermissionService) w obrębie projektu z trasy.
/// </summary>
public static class PermissionsController
{
    public static IEndpointRouteBuilder MapPermissions(this IEndpointRouteBuilder endpoints)
    {
        // Katalog uprawnień (zasób + akcja), z którego budowane są role.
        endpoints.MapGet("/api/permissions", GetPermissionsAsync)
            .WithTags(nameof(PermissionsController));

        var project = endpoints.MapGroup("/api/projects/{projectId:guid}")
            .WithTags(nameof(PermissionsController));

        project.MapGet("/roles", GetRolesAsync);
        project.MapPost("/roles", CreateRoleAsync);
        project.MapPut("/roles/{roleId:guid}", UpdateRoleAsync);
        project.MapDelete("/roles/{roleId:guid}", DeleteRoleAsync);

        project.MapGet("/members", GetMembersAsync);
        project.MapPut("/members/{userId:guid}", ChangeMemberRoleAsync);
        project.MapDelete("/members/{userId:guid}", RemoveMemberAsync);

        return endpoints;
    }

    private static Ok<IAsyncEnumerable<PermissionDto>> GetPermissionsAsync([FromServices] IMediator mediator, CancellationToken ct)
    {
        return TypedResults.Ok(mediator.CreateStream(new GetPermissionsQuery(), ct));
    }

    private static Ok<IAsyncEnumerable<ProjectRoleDto>> GetRolesAsync([FromRoute] Guid projectId,
        [FromServices] IMediator mediator, CancellationToken ct)
    {
        return TypedResults.Ok(mediator.CreateStream(new GetProjectRolesQuery
        {
            ProjectId = projectId
        }, ct));
    }

    private static async Task<Created> CreateRoleAsync([FromRoute] Guid projectId, [FromBody] ProjectRoleRequest request,
        [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new CreateProjectRoleCommand
        {
            ProjectId = projectId,
            Name = request.Name,
            IsDefault = request.IsDefault,
            PermissionIds = request.PermissionIds
        }, ct);

        return TypedResults.Created();
    }

    private static async Task<NoContent> UpdateRoleAsync([FromRoute] Guid projectId, [FromRoute] Guid roleId,
        [FromBody] ProjectRoleRequest request, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new UpdateProjectRoleCommand
        {
            ProjectId = projectId,
            RoleId = roleId,
            Name = request.Name,
            IsDefault = request.IsDefault,
            PermissionIds = request.PermissionIds
        }, ct);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteRoleAsync([FromRoute] Guid projectId, [FromRoute] Guid roleId,
        [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new DeleteProjectRoleCommand
        {
            ProjectId = projectId,
            RoleId = roleId
        }, ct);

        return TypedResults.NoContent();
    }

    private static Ok<IAsyncEnumerable<ProjectMemberDto>> GetMembersAsync([FromRoute] Guid projectId,
        [FromServices] IMediator mediator, CancellationToken ct)
    {
        return TypedResults.Ok(mediator.CreateStream(new GetProjectMembersQuery
        {
            ProjectId = projectId
        }, ct));
    }

    private static async Task<NoContent> ChangeMemberRoleAsync([FromRoute] Guid projectId, [FromRoute] Guid userId,
        [FromBody] ChangeMemberRoleRequest request, [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new ChangeProjectMemberRoleCommand
        {
            ProjectId = projectId,
            UserId = userId,
            RoleId = request.RoleId
        }, ct);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> RemoveMemberAsync([FromRoute] Guid projectId, [FromRoute] Guid userId,
        [FromServices] IMediator mediator, CancellationToken ct)
    {
        await mediator.Send(new RemoveProjectMemberCommand
        {
            ProjectId = projectId,
            UserId = userId
        }, ct);

        return TypedResults.NoContent();
    }
}
