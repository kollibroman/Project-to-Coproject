using DispatchR.Abstractions.Send;
using ProjectCowork.Domain.Shared.Aggregates;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Commands;

public record AddProjectCommand : IRequest<AddProjectCommand, Task>
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}

internal class AddProjectCommandHandler : IRequestHandler<AddProjectCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IAuthorizedUserProvider _authorizedUserProvider;
    
    public AddProjectCommandHandler(ProjectCoworkDbContext dbContext, IAuthorizedUserProvider authorizedUserProvider)
    {
        _dbContext = dbContext;
        _authorizedUserProvider = authorizedUserProvider;
    }

    public async Task Handle(AddProjectCommand request, CancellationToken cancellationToken)
    {
        var ownerId = _authorizedUserProvider.GetCurrentUserId();

        var entity = ProjectAggregate.Create(request.Name, request.Description, ownerId);
        
        _dbContext.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}