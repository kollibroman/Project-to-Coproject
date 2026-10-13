using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Repositories.Models;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Repositories.Queries;

public record GetRepositoryQuery : IRequest<GetRepositoryQuery, Task<RepositoryDto>>
{
    public required Guid RepositoryId { get; init; }
}

internal class GetRepositoryQueryHandler : IRequestHandler<GetRepositoryQuery, Task<RepositoryDto>>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public GetRepositoryQueryHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RepositoryDto> Handle(GetRepositoryQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Repositories
            .Where(x => x.Id == request.RepositoryId)
            .Select(x => new RepositoryDto
            {
                Name = x.RepositoryName
            })
            .FirstAsync(cancellationToken);
    }
}