using System.Net;
using DispatchR;
using DispatchR.Abstractions.Send;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Infrastructure.Domain.Abstractions;
using ProjectCowork.Infrastructure.Domain.Aggregates;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Commands;

/// <summary>Przyjęcie albo odrzucenie zaproszenia przez zaproszonego użytkownika.</summary>
public record AnswerInvitationToProjectCommand : IRequest<AnswerInvitationToProjectCommand, Task>
{
    public required Guid InvitationId { get; init; }
    public required bool Accept { get; init; }
}

internal class AnswerInvitationToProjectCommandHandler : IRequestHandler<AnswerInvitationToProjectCommand, Task>
{
    private readonly ProjectCoworkDbContext _context;
    private readonly IAuthorizedUserProvider _authorizedUserProvider;
    private readonly IMediator _mediator;

    public AnswerInvitationToProjectCommandHandler(ProjectCoworkDbContext context,
        IAuthorizedUserProvider authorizedUserProvider, IMediator mediator)
    {
        _context = context;
        _authorizedUserProvider = authorizedUserProvider;
        _mediator = mediator;
    }

    public async Task Handle(AnswerInvitationToProjectCommand request, CancellationToken ct)
    {
        var userId = _authorizedUserProvider.GetCurrentUserId();

        // Cudze zaproszenie traktujemy jak nieistniejące, żeby nie zdradzać jego istnienia.
        var invitation = await _context.ProjectInvitations
            .FirstOrDefaultAsync(x => x.Id == request.InvitationId && x.UserId == userId, ct);

        if (invitation is null)
        {
            throw new EntityNotFoundException($"Invitation {request.InvitationId} does not exist", HttpStatusCode.NotFound);
        }

        var aggregate = new ProjectInvitationAggregate(invitation);

        if (request.Accept)
        {
            aggregate.AcceptInvitation();
            // Handler zdarzenia dodaje członka i zapisuje zmiany razem ze stanem zaproszenia.
            await _mediator.PublishDomainEventsAsync(aggregate, ct);
        }
        else
        {
            aggregate.DeclineInvitation();
        }

        await _context.SaveChangesAsync(ct);
    }
}
