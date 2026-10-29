using System.Net;
using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Enums.Permissions;
using ProjectCowork.Domain.Exceptions.ProjectPostings;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
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
    private readonly IPermissionService _permissionService;

    public AddProjectPostingCommandHandler(ProjectCoworkDbContext dbContext, IPermissionService permissionService)
    {
        _dbContext = dbContext;
        _permissionService = permissionService;
    }

    public async Task Handle(AddProjectPostingCommand request, CancellationToken cancellationToken)
    {
        await _permissionService.EnsureProjectPermissionAsync(request.ProjectId, ResourceType.Postings,
            ActionType.Create, cancellationToken);

        var projectExists = await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.ProjectId, cancellationToken);

        if (!projectExists)
        {
            throw new ProjectPostingWihWrongProjectException("Project doesn't exist", HttpStatusCode.BadRequest);
        }

        var entity = ProjectPostingAggregate.Create(request.ProjectId, request.JobDescription,
            request.ProjectDescription);
        
        _dbContext.ProjectPostings.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
