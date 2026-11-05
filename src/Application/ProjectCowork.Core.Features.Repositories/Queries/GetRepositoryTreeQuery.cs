using System.Net;
using DispatchR.Abstractions.Send;
using ProjectCowork.Core.Features.Repositories.Abstractions;
using ProjectCowork.Core.Features.Repositories.Models;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Integrations.Git.Abstractions;
using ProjectCowork.Infrastructure.Integrations.Git.Abstractions.Models;

namespace ProjectCowork.Core.Features.Repositories.Queries;

public record GetRepositoryTreeQuery : IRequest<GetRepositoryTreeQuery, Task<IEnumerable<TreeEntryDto>>>
{
    public required Guid RepositoryId { get; init; }
    public required string Branch { get; init; }
    public string Path { get; init; } = string.Empty;
}

internal class GetRepositoryTreeQueryHandler : IRequestHandler<GetRepositoryTreeQuery, Task<IEnumerable<TreeEntryDto>>>
{
    private readonly IRepositoryAccessService _repositoryAccessService;
    private readonly IGitBrowserService _gitBrowserService;

    public GetRepositoryTreeQueryHandler(IRepositoryAccessService repositoryAccessService,
        IGitBrowserService gitBrowserService)
    {
        _repositoryAccessService = repositoryAccessService;
        _gitBrowserService = gitBrowserService;
    }

    public async Task<IEnumerable<TreeEntryDto>> Handle(GetRepositoryTreeQuery request, CancellationToken cancellationToken)
    {
        var repository = await _repositoryAccessService.GetViewableRepositoryAsync(request.RepositoryId, cancellationToken);

        var tree = _gitBrowserService.GetTree(repository.ProjectId, repository.Name, request.Branch, request.Path)
                   ?? throw new EntityNotFoundException(
                       $"Folder '{request.Path}' on branch '{request.Branch}' does not exist", HttpStatusCode.NotFound);

        return tree.Select(e => new TreeEntryDto
        {
            Name = e.Name,
            Path = e.Path,
            Type = e.Type == GitTreeItemType.Folder ? TreeEntryType.Folder : TreeEntryType.File,
            Size = e.Size
        }).ToList();
    }
}
