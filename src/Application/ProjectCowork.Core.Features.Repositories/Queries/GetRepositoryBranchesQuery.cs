using System.Net;
using DispatchR.Abstractions.Send;
using ProjectCowork.Core.Features.Repositories.Abstractions;
using ProjectCowork.Core.Features.Repositories.Models;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Integrations.Git.Abstractions;

namespace ProjectCowork.Core.Features.Repositories.Queries;

public record GetRepositoryBranchesQuery : IRequest<GetRepositoryBranchesQuery, Task<IEnumerable<BranchDto>>>
{
    public required Guid RepositoryId { get; init; }
}

internal class GetRepositoryBranchesQueryHandler : IRequestHandler<GetRepositoryBranchesQuery, Task<IEnumerable<BranchDto>>>
{
    private readonly IRepositoryAccessService _repositoryAccessService;
    private readonly IGitBrowserService _gitBrowserService;

    public GetRepositoryBranchesQueryHandler(IRepositoryAccessService repositoryAccessService,
        IGitBrowserService gitBrowserService)
    {
        _repositoryAccessService = repositoryAccessService;
        _gitBrowserService = gitBrowserService;
    }

    public async Task<IEnumerable<BranchDto>> Handle(GetRepositoryBranchesQuery request, CancellationToken cancellationToken)
    {
        var repository = await _repositoryAccessService.GetViewableRepositoryAsync(request.RepositoryId, cancellationToken);

        var branches = _gitBrowserService.GetBranches(repository.ProjectId, repository.Name)
                       ?? throw new EntityNotFoundException(
                           $"Repository {request.RepositoryId} does not exist on disk", HttpStatusCode.NotFound);

        return branches.Select(b => new BranchDto
        {
            Name = b.Name,
            IsDefault = b.IsDefault
        }).ToList();
    }
}
