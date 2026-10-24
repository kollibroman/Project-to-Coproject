using DispatchR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ProjectCowork.Api.Models;
using ProjectCowork.Api.Utils;
using ProjectCowork.Core.Features.User.Commands;
using ProjectCowork.Core.Features.User.Models;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Exceptions.Auth;

namespace ProjectCowork.Api.Controllers;

public static class AuthController
{
    public static IEndpointRouteBuilder AddAuth(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth");
        
        group.MapPost("/login", LoginAsync);
        group.MapPost("/register/user", RegisterAsUserAsync);
        group.MapPost("/register/business", RegisterAsBusinessAsync);
        group.MapPost("/token/refresh", RefreshTokenAsync);
        group.MapPost("/logout", LogoutAsync);
        
        return endpoints;
    }
    
    private static async Task<Ok<AccessTokenResponse>> LoginAsync([FromBody] LoginRequest request, HttpResponse response,
        [FromServices] RefreshTokenCookie cookie, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        }, ct);
        
        return IssueTokens(result, response, cookie);
    }
    
    private static async Task<Ok<AccessTokenResponse>> RegisterAsUserAsync([FromBody] RegisterRequest request, HttpResponse response,
        [FromServices] RefreshTokenCookie cookie, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new RegisterCommand
        {
            Email = request.Email,
            Password = request.Password,
            Username = request.Username,
            UserRole = RoleNames.User
        }, ct);
        
        return IssueTokens(result, response, cookie);
    }
    
    private static async Task<Ok<AccessTokenResponse>> RegisterAsBusinessAsync([FromBody] RegisterRequest request, HttpResponse response,
        [FromServices] RefreshTokenCookie cookie, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new RegisterCommand
        {
            Email = request.Email,
            Password = request.Password,
            Username = request.Username,
            UserRole = RoleNames.Business
        }, ct);
        
        return IssueTokens(result, response, cookie);
    }
    
    private static async Task<Results<Ok<AccessTokenResponse>, UnauthorizedHttpResult>> RefreshTokenAsync(HttpRequest request, HttpResponse response,
        [FromServices] RefreshTokenCookie cookie, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var refreshToken = RefreshTokenCookie.Read(request);

        if (refreshToken is null)
        {
            return TypedResults.Unauthorized();
        }

        try
        {
            var result = await mediator.Send(new RefreshTokenCommand
            {
                RefreshToken = refreshToken
            }, ct);

            return IssueTokens(result, response, cookie);
        }
        catch (InvalidTokenException)
        {
            // Nieważny lub wygasły token — usuwamy ciasteczko, żeby przeglądarka go nie odsyłała.
            cookie.Delete(response);
            return TypedResults.Unauthorized();
        }
    }

    private static async Task<NoContent> LogoutAsync(HttpRequest request, HttpResponse response,
        [FromServices] RefreshTokenCookie cookie, [FromServices] IMediator mediator, CancellationToken ct)
    {
        var refreshToken = RefreshTokenCookie.Read(request);

        if (refreshToken is not null)
        {
            await mediator.Send(new LogoutCommand
            {
                RefreshToken = refreshToken
            }, ct);
        }

        cookie.Delete(response);
        return TypedResults.NoContent();
    }

    private static Ok<AccessTokenResponse> IssueTokens(TokenDto tokens, HttpResponse response, RefreshTokenCookie cookie)
    {
        cookie.Append(response, tokens.RefreshToken, tokens.RefreshTokenExpiresAt);

        return TypedResults.Ok(new AccessTokenResponse
        {
            AccessToken = tokens.AccessToken
        });
    }
}
