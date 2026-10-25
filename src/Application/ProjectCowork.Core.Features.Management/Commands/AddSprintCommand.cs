using System.Net;
using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Management.Commands;

public record AddSprintCommand : IRequest<AddSprintCommand, Task>
{
    public required Guid ProjectId { get; init; }
    public required string Name { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
}

internal class AddSprintCommandHandler : IRequestHandler<AddSprintCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;

    public AddSprintCommandHandler(ProjectCoworkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(AddSprintCommand request, CancellationToken ct)
    {
        var projectExists = await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.ProjectId, ct);

        if (!projectExists)
        {
            throw new EntityNotFoundException($"Project {request.ProjectId} does not exist", HttpStatusCode.BadRequest);
        }

        var entity = SprintAggregate.Create(request.ProjectId, request.Name, request.StartDate, request.EndDate);

        _dbContext.Sprints.Add(entity);
        await _dbContext.SaveChangesAsync(ct);
    }
}
