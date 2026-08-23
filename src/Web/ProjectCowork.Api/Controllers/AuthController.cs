using DispatchR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProjectCowork.Api.Models;
using ProjectCowork.Core.Features.User.Commands;
using ProjectCowork.Core.Features.User.Models;
using ProjectCowork.Domain.Enums.Permissions;

namespace ProjectCowork.Api.Controllers;

public static class AuthController
{
    public static IEndpointRouteBuilder AddAuth(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth");
        
        group.MapPost("/login", LoginAsync);
        group.MapPost("/register/user", RegisterAsUserAsync);
        group.MapPost("/register/business", RegisterAsBusinessAsync);

        return endpoints;
    }
    
    private static async Task<Ok<TokenDto>> LoginAsync([FromBody] LoginRequest request,[FromServices] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        }, ct);
        
        return TypedResults.Ok(result);
    }
    
    private static async Task<Ok<TokenDto>> RegisterAsUserAsync([FromBody] RegisterRequest request,
        [FromServices] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new RegisterCommand
        {
            Email = request.Email,
            Password = request.Password,
            Username = request.Username,
            UserRole = RoleNames.User
        }, ct);
        
        return TypedResults.Ok(result);
    }
    
    private static async Task<Ok<TokenDto>> RegisterAsBusinessAsync([FromBody] RegisterRequest request,
        [FromServices] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new RegisterCommand
        {
            Email = request.Email,
            Password = request.Password,
            Username = request.Username,
            UserRole = RoleNames.Business
        }, ct);
        
        return TypedResults.Ok(result);
    }
}