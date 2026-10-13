using System.Net;
using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Repositories.Commands;

public record DeleteRepositoryCommand : IRequest<DeleteRepositoryCommand, Task>
{
    public required Guid RepositoryId { get; init; }
}

internal class DeleteRepositoryCommandHandler : IRequestHandler<DeleteRepositoryCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public DeleteRepositoryCommandHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteRepositoryCommand request, CancellationToken cancellationToken)
    {
        var repository = await _dbContext.Repositories
            .Include(x => x.Project)
            .ThenInclude(x => x.Repositories)
            .FirstOrDefaultAsync(x => x.Id == request.RepositoryId, cancellationToken);

        if (repository == null)
        {
            throw new EntityNotFoundException("Entity not found", HttpStatusCode.NotFound);
        }

        var aggregate = new RepositoryAggregate(repository, repository.Project);
        aggregate.DeleteRepositoryAsync();
        
        _dbContext.Remove(repository);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}