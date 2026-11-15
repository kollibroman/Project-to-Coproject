using DispatchR.Abstractions.Notification;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Infrastructure.DomainEvents.Projects;
using ProjectCowork.Infrastructure.Integrations.Email.Abstractions;
using ProjectCowork.Infrastructure.Integrations.Email.Abstractions.Models;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Integrations.DomainEvents;

internal class InvitationCreatedEventHandler : INotificationHandler<InvitationCreatedEvent>
{
    private ProjectCoworkDbContext _dbContext;
    private readonly IMailService _mailService;
    
    public InvitationCreatedEventHandler(ProjectCoworkDbContext dbContext, IMailService mailService)
    {
        _dbContext = dbContext;
        _mailService = mailService;
    }

    public async ValueTask Handle(InvitationCreatedEvent request, CancellationToken ct)
    {
        var invitation = await _dbContext.ProjectInvitations
            .Where(x => x.Id == request.InvitationId)
            .Select(x => new
            {
                x.User.Email,
            })
            .FirstOrDefaultAsync(ct);

        var emailModel = new SendEmailModel
        {
            Receiver = invitation.Email,
            MessageContent = "Test Content"
        };
        
        await _mailService.SendEmailAsync(emailModel, ct);
    }
}