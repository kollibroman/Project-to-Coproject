using System.Net;
using DispatchR.Abstractions.Send;
using ProjectCowork.Core.Features.Repositories.Abstractions;
using ProjectCowork.Core.Features.Repositories.Models;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Integrations.Git.Abstractions;

namespace ProjectCowork.Core.Features.Repositories.Queries;

public record GetRepositoryCommitsQuery : IRequest<GetRepositoryCommitsQuery, Task<CommitsPageDto>>
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public required Guid RepositoryId { get; init; }
    public required string Branch { get; init; }

    /// <summary>Numer strony liczony od 1.</summary>
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = DefaultPageSize;
}

internal class GetRepositoryCommitsQueryHandler : IRequestHandler<GetRepositoryCommitsQuery, Task<CommitsPageDto>>
{
    private readonly IRepositoryAccessService _repositoryAccessService;
    private readonly IGitBrowserService _gitBrowserService;

    public GetRepositoryCommitsQueryHandler(IRepositoryAccessService repositoryAccessService,
        IGitBrowserService gitBrowserService)
    {
        _repositoryAccessService = repositoryAccessService;
        _gitBrowserService = gitBrowserService;
    }

    public async Task<CommitsPageDto> Handle(GetRepositoryCommitsQuery request, CancellationToken cancellationToken)
    {
        var repository = await _repositoryAccessService.GetViewableRepositoryAsync(request.RepositoryId, cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, GetRepositoryCommitsQuery.MaxPageSize);

        var commits = _gitBrowserService.GetCommits(repository.ProjectId, repository.Name, request.Branch, page,
                          pageSize, cancellationToken)
                      ?? throw new EntityNotFoundException(
                          $"Branch '{request.Branch}' does not exist", HttpStatusCode.NotFound);

        return new CommitsPageDto
        {
            Items = commits.Items.Select(c => new CommitDto
            {
                Sha = c.Sha,
                Message = c.Message,
                AuthorName = c.AuthorName,
                AuthorEmail = c.AuthorEmail,
                Date = c.Date
            }).ToList(),
            HasMore = commits.HasMore
        };
    }
}
