using DispatchR.Abstractions.Send;
using ProjectCowork.Infrastructure.Authorization.Abstractions;

namespace ProjectCowork.Core.Features.User.Commands;

public record LogoutCommand : IRequest<LogoutCommand, Task>
{
    public required string RefreshToken { get; init; }
}

internal sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Task>
{
    private readonly ITokenService _tokenService;

    public LogoutCommandHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);
    }
}
