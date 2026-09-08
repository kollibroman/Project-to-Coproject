using DispatchR.Abstractions.Notification;
using Microsoft.Extensions.Logging;
using ProjectCowork.Infrastructure.Integrations.AzureBlob.Abstractions;
using ProjectCowork.Infrastructure.Integrations.AzureBlob.Models;

namespace ProjectCowork.Infrastructure.DomainEvents.Attachments;

public sealed record ManyAttachmentsAddedEvent : INotification
{
    public required IEnumerable<AttachmentAddedEvent> Events { get; init; }
}

internal sealed class ManyAttachmentsAddedEventHandler : INotificationHandler<ManyAttachmentsAddedEvent>
{
    private readonly IBlobService _blobService;
    private readonly ILogger<AttachmentAddedEventHandler> _logger;

    public ManyAttachmentsAddedEventHandler(IBlobService blobService, ILogger<AttachmentAddedEventHandler> logger)
    {
        _blobService = blobService;
        _logger = logger;
    }

    public async ValueTask Handle(ManyAttachmentsAddedEvent request, CancellationToken cancellationToken)
    {
        var uploadTaskList = request.Events.Select(@event => new SaveFileRequest
            {
                // TODO: generic name
                ContainerName = "Attachments",
                FileName = @event.FileName,
                Content = @event.ContentStream
            })
            .Select(saveFileRequest => _blobService.UploadFileASync(saveFileRequest, cancellationToken))
            .ToList();

        await Task.WhenAll(uploadTaskList);
    }
}