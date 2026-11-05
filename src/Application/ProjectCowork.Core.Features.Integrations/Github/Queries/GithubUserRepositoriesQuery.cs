using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Integrations.Github.Models;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Integrations.Github.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Integrations.Github.Queries;

public record GithubUserRepositoriesQuery : IStreamRequest<GithubUserRepositoriesQuery, UserRepositoryDto>;

internal class GithubUserRepositoriesQueryHandler : IStreamRequestHandler<GithubUserRepositoriesQuery, UserRepositoryDto>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IAuthorizedUserProvider _authorizedUserProvider;
    private readonly IGithubService _githubService;

    public GithubUserRepositoriesQueryHandler(ProjectCoworkDbContext dbContext, IAuthorizedUserProvider authorizedUserProvider, IGithubService githubService)
    {
        _dbContext = dbContext;
        _authorizedUserProvider = authorizedUserProvider;
        _githubService = githubService;
    }

    public async IAsyncEnumerable<UserRepositoryDto> Handle(GithubUserRepositoriesQuery request, CancellationToken ct)
    {
        var userId = _authorizedUserProvider.GetCurrentUserId();

        var userGithubData = await _dbContext.Users
            .Where(x => x.Id == userId)
            .Select(x => new
            {
                x.GithubFineGrainedPat,
                x.GithubUsername
            })
            .FirstOrDefaultAsync(ct);

        if (userGithubData?.GithubFineGrainedPat == null || userGithubData.GithubUsername == null)
        {
            throw new EntityNotFoundException("User not found");
        }
        
        var apiResp = await _githubService.FetchUserRepositoriesAsync(userGithubData.GithubUsername, userGithubData.GithubFineGrainedPat, ct);

        if (apiResp is null)
        {
            throw new InvalidOperationException();
        }

        foreach (var resp in apiResp)
        {
            yield return new UserRepositoryDto
            {
                Name = resp.FullName,
                CloneUrl = resp.CloneUrl,
                OwnerName = resp.Owner.Login,
            };
        }
    }
}