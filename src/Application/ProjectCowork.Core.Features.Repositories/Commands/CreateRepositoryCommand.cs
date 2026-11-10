using DispatchR.Abstractions.Send;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Infrastructure.Integrations.Git.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Repositories.Commands;

public record CreateRepositoryCommand : IRequest<CreateRepositoryCommand, Task<string>>
{
    public required string Name { get; init; }
    public required Guid ProjectId { get; init; }
}

internal class CreateRepositoryCommandHandler : IRequestHandler<CreateRepositoryCommand, Task<string>>
{
    private readonly ProjectCoworkDbContext _projectCoworkDbContext;
    private readonly IGitClientService _gitClientService;
    private readonly IPermissionService _permissionService;

    public CreateRepositoryCommandHandler(ProjectCoworkDbContext projectCoworkDbContext, IGitClientService gitClientService,
        IPermissionService permissionService)
    {
        _projectCoworkDbContext = projectCoworkDbContext;
        _gitClientService = gitClientService;
        _permissionService = permissionService;
    }

    public async Task<string> Handle(CreateRepositoryCommand request, CancellationToken ct)
    {
        // Przed utworzeniem repozytorium na dysku, żeby odmowa nie zostawiała plików.
        await _permissionService.EnsureProjectPermissionAsync(request.ProjectId, ResourceType.Repositories, ActionType.Create, ct);

        var clonePath = await _gitClientService.InitBareRepositoryAsync(request.Name, request.ProjectId);

        var repository = RepositoryAggregate.Create(request.Name, clonePath, request.ProjectId);

        _projectCoworkDbContext.Add(repository);
        await _projectCoworkDbContext.SaveChangesAsync(ct);

        return clonePath;
    }
}
