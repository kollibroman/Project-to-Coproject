using System.Net;
using DispatchR.Abstractions.Send;
using ProjectCowork.Core.Features.Repositories.Abstractions;
using ProjectCowork.Core.Features.Repositories.Models;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Integrations.Git.Abstractions;

namespace ProjectCowork.Core.Features.Repositories.Queries;

public record GetRepositoryFileQuery : IRequest<GetRepositoryFileQuery, Task<FileContentDto>>
{
    public required Guid RepositoryId { get; init; }
    public required string Branch { get; init; }
    public required string Path { get; init; }
}

internal class GetRepositoryFileQueryHandler : IRequestHandler<GetRepositoryFileQuery, Task<FileContentDto>>
{
    private readonly IRepositoryAccessService _repositoryAccessService;
    private readonly IGitBrowserService _gitBrowserService;

    public GetRepositoryFileQueryHandler(IRepositoryAccessService repositoryAccessService,
        IGitBrowserService gitBrowserService)
    {
        _repositoryAccessService = repositoryAccessService;
        _gitBrowserService = gitBrowserService;
    }

    public async Task<FileContentDto> Handle(GetRepositoryFileQuery request, CancellationToken cancellationToken)
    {
        var repository = await _repositoryAccessService.GetViewableRepositoryAsync(request.RepositoryId, cancellationToken);

        var file = _gitBrowserService.GetFile(repository.ProjectId, repository.Name, request.Branch, request.Path)
                   ?? throw new EntityNotFoundException(
                       $"File '{request.Path}' on branch '{request.Branch}' does not exist", HttpStatusCode.NotFound);

        return new FileContentDto
        {
            Path = file.Path,
            Content = file.Content,
            IsBinary = file.IsBinary,
            Size = file.Size
        };
    }
}
