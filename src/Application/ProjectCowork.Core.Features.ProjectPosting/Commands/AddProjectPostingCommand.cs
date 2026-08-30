using DispatchR.Abstractions.Send;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.ProjectPosting.Commands;

public record AddProjectPostingCommand : IRequest<AddProjectPostingCommand, Task>
{
    public required Guid ProjectId { get; init; }
    public required string JobDescription { get; init; }
    public required string ProjectDescription { get; init; }
}

internal class AddProjectPostingCommandHandler : IRequestHandler<AddProjectPostingCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public AddProjectPostingCommandHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(AddProjectPostingCommand request, CancellationToken cancellationToken)
    {
        var entity = await ProjectPostingAggregate.Create(_dbContext, request.ProjectId, request.JobDescription,
            request.ProjectDescription);
        
        _dbContext.ProjectPostings.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}