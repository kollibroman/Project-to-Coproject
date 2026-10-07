using DispatchR.Abstractions.Send;
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

    public CreateRepositoryCommandHandler(ProjectCoworkDbContext projectCoworkDbContext, IGitClientService gitClientService)
    {
        _projectCoworkDbContext = projectCoworkDbContext;
        _gitClientService = gitClientService;
    }

    public async Task<string> Handle(CreateRepositoryCommand request, CancellationToken ct)
    {
        var repository = RepositoryAggregate.Create(request.Name,string.Empty, request.ProjectId);
        
        var clonePath = await _gitClientService.InitBareRepositoryAsync(request.Name, repository.Id);

        repository.RepoPath = clonePath;
        _projectCoworkDbContext.Add(repository);
        await _projectCoworkDbContext.SaveChangesAsync(ct);

        return clonePath;
    }
}