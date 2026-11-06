using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Integrations.Github.Commands;

public record GithubAuthorizeWithPatCommand : IRequest<GithubAuthorizeWithPatCommand, Task>
{
    public required string GithubUsername { get; init; }
    public required string Pat { get; init; }
}

internal class GithubAuthorizeWithPatCommandHandler : IRequestHandler<GithubAuthorizeWithPatCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IAuthorizedUserProvider _authorizedUserProvider;

    public GithubAuthorizeWithPatCommandHandler(ProjectCoworkDbContext dbContext, IAuthorizedUserProvider authorizedUserProvider)
    {
        _dbContext = dbContext;
        _authorizedUserProvider = authorizedUserProvider;
    }

    public async Task Handle(GithubAuthorizeWithPatCommand request, CancellationToken ct)
    {
        var userId = _authorizedUserProvider.GetCurrentUserId();

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);

        if (user is null)
        {
            throw new EntityNotFoundException("User not found");
        }
        
        user.GithubFineGrainedPat = request.Pat;
        user.GithubUsername = request.GithubUsername;
        await _dbContext.SaveChangesAsync(ct);
    }
}