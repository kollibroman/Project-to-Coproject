using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.ProjectPosting.Commands;

public record UpdateProjectPostingCommand : IRequest<UpdateProjectPostingCommand, Task>
{
    public required Guid ProjectPostingId { get; init; }
    public required string JobDescription { get; init; }
    public required string ProjectDescription { get; init; }
}

internal class UpdateProjectPostingCommandHandler : IRequestHandler<UpdateProjectPostingCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public UpdateProjectPostingCommandHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateProjectPostingCommand request, CancellationToken ct)
    {
        var entity = await _dbContext.ProjectPostings.FirstOrDefaultAsync(x => x.Id == request.ProjectPostingId, ct);

        if (entity is null)
        {
            throw new EntityNotFoundException($"ProjectPosting {request.ProjectPostingId} does not exist");
        }

        var aggregate = new ProjectPostingAggregate(entity);
        
        aggregate.SetJobDescription(request.JobDescription);
        aggregate.SetProjectDescription(request.ProjectDescription);
        
        await _dbContext.SaveChangesAsync(ct);
    }
}