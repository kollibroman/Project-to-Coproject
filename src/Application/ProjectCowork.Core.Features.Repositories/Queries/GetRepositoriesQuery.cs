using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Repositories.Models;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Repositories.Queries;

public record GetRepositoriesQuery : IRequest<GetRepositoriesQuery, Task<IEnumerable<RepositoryDto>>>
{
    public required Guid ProjectId { get; init; }
}

internal class GetRepositoriesQueryHandler : IRequestHandler<GetRepositoriesQuery, Task<IEnumerable<RepositoryDto>>>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public GetRepositoriesQueryHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<RepositoryDto>> Handle(GetRepositoriesQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Repositories
            .Where(x => x.ProjectId == request.ProjectId)
            .Select(x => new RepositoryDto
            {
                Name = x.RepositoryName,
                Id = x.Id,
                RepoUrl = x.RepoPath
            })
            .ToListAsync(cancellationToken);
    }
}