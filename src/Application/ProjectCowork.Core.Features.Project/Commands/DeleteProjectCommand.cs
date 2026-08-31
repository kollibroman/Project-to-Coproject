using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Commands;

public record DeleteProjectCommand : IRequest<DeleteProjectCommand, Task>
{
    public required Guid ProjectId { get; init; }
}

internal class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Task>
{
    private readonly ProjectCoworkDbContext _context;

    public DeleteProjectCommandHandler(ProjectCoworkDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteProjectCommand request, CancellationToken ct)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == request.ProjectId, ct);

        var aggregate = new ProjectAggregate(project);
        
        aggregate.DeleteProject();

        _context.Remove(project);
        await _context.SaveChangesAsync(ct);
    }
}