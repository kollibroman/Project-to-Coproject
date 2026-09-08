using DispatchR.Abstractions.Notification;
using Microsoft.Extensions.Logging;
using ProjectCowork.Infrastructure.Integrations.AzureBlob.Abstractions;
using ProjectCowork.Infrastructure.Integrations.AzureBlob.Models;

namespace ProjectCowork.Infrastructure.DomainEvents.Attachments;

public record AttachmentAddedEvent : INotification
{
   public required Stream ContentStream { get; init; }
   public required string FileName { get; init; }
   public required string ContentType { get; init; }
}

internal class AttachmentAddedEventHandler : INotificationHandler<AttachmentAddedEvent>
{
    private readonly IBlobService _blobService;
    private readonly ILogger<AttachmentAddedEventHandler> _logger;

    public AttachmentAddedEventHandler(IBlobService blobService, ILogger<AttachmentAddedEventHandler> logger)
    {
        _blobService = blobService;
        _logger = logger;
    }

    public async ValueTask Handle(AttachmentAddedEvent request, CancellationToken cancellationToken)
    {
        try
        {
            var saveFileRequest = new SaveFileRequest
            {
                // TODO: generic name
                ContainerName = "Attachments",
                FileName = request.FileName,
                Content = request.ContentStream
            };
            
            await _blobService.UploadFileASync(saveFileRequest, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }
}