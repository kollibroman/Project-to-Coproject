using DispatchR.Abstractions.Send;
using Microsoft.Extensions.Logging;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Infrastructure.Integrations.Git.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Integrations.Github.Commands;

public record GithubImportProjectCommand : IRequest<GithubImportProjectCommand, Task<string>>
{
    public required Guid ProjectId { get; init; }
    public required string RepositoryName { get; init; }
    public required string CloneUrl { get; init; }
}

internal class GithubImportProjectCommandHandler : IRequestHandler<GithubImportProjectCommand, Task<string>>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly ILogger<GithubImportProjectCommandHandler> _logger;
    private readonly IGitClientService _gitClientService;
    private readonly IPermissionService _permissionService;
    
    public GithubImportProjectCommandHandler(ProjectCoworkDbContext dbContext, ILogger<GithubImportProjectCommandHandler> logger, IGitClientService gitClientService, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _gitClientService = gitClientService;
        _permissionService = permissionService;
    }

    public async Task<string> Handle(GithubImportProjectCommand request, CancellationToken ct)
    {
        await _permissionService.EnsureProjectPermissionAsync(request.ProjectId, ResourceType.Repositories, ActionType.Create, ct);
        
        var localClonePath = await _gitClientService.CloneGithubRepositoryAsync(request.CloneUrl, request.RepositoryName, request.ProjectId);
        var repository = RepositoryAggregate.Create(request.RepositoryName, localClonePath, request.ProjectId);
        
        _dbContext.Add(repository);
        await _dbContext.SaveChangesAsync(ct);

        return localClonePath;
    }
}