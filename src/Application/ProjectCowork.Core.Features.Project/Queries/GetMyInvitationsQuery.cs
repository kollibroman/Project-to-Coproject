using DispatchR.Abstractions.Stream;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Core.Features.Models;
using ProjectCowork.Infrastructure.Authorization.Abstractions;
using ProjectCowork.Persistence;

namespace ProjectCowork.Core.Features.Queries;

/// <summary>Oczekujące zaproszenia do projektów dla zalogowanego użytkownika, od najnowszych.</summary>
public record GetMyInvitationsQuery : IStreamRequest<GetMyInvitationsQuery, ProjectInvitationModel>;

internal class GetMyInvitationsQueryHandler : IStreamRequestHandler<GetMyInvitationsQuery, ProjectInvitationModel>
{
    private readonly ProjectCoworkDbContext _context;
    private readonly IAuthorizedUserProvider _authorizedUserProvider;

    public GetMyInvitationsQueryHandler(ProjectCoworkDbContext context, IAuthorizedUserProvider authorizedUserProvider)
    {
        _context = context;
        _authorizedUserProvider = authorizedUserProvider;
    }

    public IAsyncEnumerable<ProjectInvitationModel> Handle(GetMyInvitationsQuery request, CancellationToken ct)
    {
        var userId = _authorizedUserProvider.GetCurrentUserId();

        return _context.ProjectInvitations
            .Where(x => x.UserId == userId && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ProjectInvitationModel
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                ProjectName = x.Project.Name,
                ProjectDescription = x.Project.Description,
                InvitedBy = x.Project.Owner.UserName ?? x.Project.Owner.Email ?? string.Empty,
                InvitationContent = x.InvitationContent,
                RoleName = x.RoleId == null
                    ? null
                    : x.Project.ProjectRoles
                        .Where(role => role.Id == x.RoleId)
                        .Select(role => role.Name)
                        .FirstOrDefault(),
                CreatedAt = x.CreatedAt
            })
            .AsAsyncEnumerable();
    }
}
