using System.Net;
using DispatchR;
using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Exceptions.ProjectPostings;
using ProjectCowork.Domain.Models.Attachments;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Domain.Abstractions;
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

        var posting = await _dbContext.ProjectPostings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ProjectPostingId, cancellationToken);

        if (posting is null)
        {
            throw new ProjectApplicationToNonExistentPostingException(
                $"Posting {request.ProjectPostingId} doesn't exist", HttpStatusCode.BadRequest);
        }

        var entity = ProjectApplicationAggregate.Create(posting, request.Description, userId);

        var attachmentList = new List<AttachmentEntity>();
        var attachmentAggregates = new List<AttachmentAggregate>();
        
        foreach (var attachment in request.Attachments)
        {
            // TODO: Add attachment name generator
            var file = AttachmentAggregate.Create(attachment.FileName, attachment.FileName, attachment.SizeInBytes);
            attachmentList.Add(file);
            
            var attachmentAggregate = new AttachmentAggregate(file, attachment.ContentStream, attachment.ContentType);
            attachmentAggregate.ScheduleBlobUpload();
            attachmentAggregates.Add(attachmentAggregate);
        }
        
        var aggregate = new ProjectApplicationAggregate(entity, attachmentList);
        aggregate.SetApplicationAttachments();

        _dbContext.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _mediator.PublishDomainEventsAsync(attachmentAggregates, cancellationToken);
    }
}
