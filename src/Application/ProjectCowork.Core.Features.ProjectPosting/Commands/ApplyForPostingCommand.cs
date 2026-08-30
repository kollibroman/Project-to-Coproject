using DispatchR.Abstractions.Send;
using ProjectCowork.Domain.Models.Attachments;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.ProjectPosting.Commands;

public record ApplyForPostingCommand : IRequest<ApplyForPostingCommand, Task>
{
    public required Guid ProjectPostingId { get; init; }
    public required string Description { get; init; }
    public required IEnumerable<AttachmentModel> Attachments { get; init; }
    
    public record AttachmentModel
    {
        public required string FileName { get; init; }
        public required int SizeInBytes { get; init; }
        public required string BlobUrl { get; init; }
    }
}

internal class ApplyForPostingCommandHandler : IRequestHandler<ApplyForPostingCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IAuthorizedUserProvider _authorizedUserProvider;

    public ApplyForPostingCommandHandler(ProjectCoworkDbContext dbContext, IAuthorizedUserProvider authorizedUserProvider)
    {
        _dbContext = dbContext;
        _authorizedUserProvider = authorizedUserProvider;
    }

    public async Task Handle(ApplyForPostingCommand request, CancellationToken cancellationToken)
    {
        var userId = _authorizedUserProvider.GetCurrentUserId();
        
        var entity = await ProjectApplicationAggregate.Create(_dbContext, request.Description, userId, request.ProjectPostingId);

        var attachmentList = new List<AttachmentEntity>();

        foreach (var attachment in request.Attachments)
        {
            // TODO: Add attachmentName generator
            attachmentList.Add(AttachmentAggregate.Create(attachment.FileName, attachment.FileName, attachment.SizeInBytes));
        }
        
        var aggregate = new ProjectApplicationAggregate(entity, attachmentList);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}