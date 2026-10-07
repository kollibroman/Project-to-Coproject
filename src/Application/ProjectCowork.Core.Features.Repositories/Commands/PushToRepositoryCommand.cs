using DispatchR.Abstractions.Send;
using ProjectCowork.Infrastructure.Integrations.Git.Abstractions;

namespace ProjectCowork.Core.Features.Repositories.Commands;

public record PushToRepositoryCommand : IRequest<PushToRepositoryCommand, Task>
{
    public required string Name { get; init; }
}

internal class PushToRepositoryCommandHandler : IRequestHandler<PushToRepositoryCommand, Task>
{
    private readonly IGitServerService _gitServerService;
    
    public PushToRepositoryCommandHandler(IGitServerService gitServerService)
    {
        _gitServerService = gitServerService;
    }

    public async Task Handle(PushToRepositoryCommand request, CancellationToken ct)
    {
        await _gitServerService.UploadAndCreateRemoteRepositoryAsync(request.Name, ct);
    }
}