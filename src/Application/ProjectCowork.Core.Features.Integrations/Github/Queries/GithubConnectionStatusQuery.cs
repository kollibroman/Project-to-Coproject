using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Integrations.Github.Models;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Integrations.Github.Queries;

public record GithubConnectionStatusQuery : IRequest<GithubConnectionStatusQuery, Task<GithubConnectionStatusDto>>;

internal class GithubConnectionStatusQueryHandler : IRequestHandler<GithubConnectionStatusQuery, Task<GithubConnectionStatusDto>>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IAuthorizedUserProvider _authorizedUserProvider;

    public GithubConnectionStatusQueryHandler(ProjectCoworkDbContext dbContext, IAuthorizedUserProvider authorizedUserProvider)
    {
        _dbContext = dbContext;
        _authorizedUserProvider = authorizedUserProvider;
    }

    public async Task<GithubConnectionStatusDto> Handle(GithubConnectionStatusQuery request, CancellationToken ct)
    {
        var userId = _authorizedUserProvider.GetCurrentUserId();

        var githubData = await _dbContext.Users
            .Where(x => x.Id == userId)
            .Select(x => new
            {
                x.GithubUsername,
                HasPat = x.GithubFineGrainedPat != null
            })
            .FirstOrDefaultAsync(ct);

        if (githubData is null)
        {
            throw new EntityNotFoundException("User not found");
        }

        return new GithubConnectionStatusDto
        {
            IsConnected = githubData.HasPat && githubData.GithubUsername != null,
            GithubUsername = githubData.GithubUsername
        };
    }
}
