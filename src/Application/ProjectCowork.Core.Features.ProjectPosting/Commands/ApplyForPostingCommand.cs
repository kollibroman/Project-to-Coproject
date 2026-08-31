using DispatchR;
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
        public required Stream ContentStream { get; init; }
        public required long SizeInBytes { get; init; }
        public required string ContentType { get; init; }
    }
}

internal class ApplyForPostingCommandHandler : IRequestHandler<ApplyForPostingCommand, Task>
{
    private readonly ProjectCoworkDbContext _dbContext;
    private readonly IAuthorizedUserProvider _authorizedUserProvider;
    private readonly IMediator _mediator;
    
    public ApplyForPostingCommandHandler(ProjectCoworkDbContext dbContext, IAuthorizedUserProvider authorizedUserProvider, IMediator mediator)
    {
        _dbContext = dbContext;
        _authorizedUserProvider = authorizedUserProvider;
        _mediator = mediator;
    }

    public async Task Handle(ApplyForPostingCommand request, CancellationToken cancellationToken)
    {
        var userId = _authorizedUserProvider.GetCurrentUserId();
        
        var entity = await ProjectApplicationAggregate.Create(_dbContext, request.Description, userId, request.ProjectPostingId);

        var attachmentList = new List<AttachmentEntity>();
        var setBlobTasks = new List<Task>();
        
        foreach (var attachment in request.Attachments)
        {
            // TODO: Add attachment name generator
            var file = AttachmentAggregate.Create(attachment.FileName, attachment.FileName, attachment.SizeInBytes);
            attachmentList.Add(file);
            
            var attachmentAggregate = new AttachmentAggregate(file, attachment.ContentStream, attachment.ContentType);
            setBlobTasks.Add(attachmentAggregate.SetBlobAsync(_mediator, cancellationToken));
        }
        
        var aggregate = new ProjectApplicationAggregate(entity, attachmentList);
        aggregate.SetApplicationAttachmentsAsync();
        await Task.WhenAll(setBlobTasks);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}