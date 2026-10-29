using ProjectCowork.Domain.Enums.Permissions;

namespace ProjectCowork.Infrastructure.Authorization.Abstractions;

/// <summary>
/// Uprawnienia w obrębie projektu: właściciel ma wszystkie, członek ma uprawnienia swojej roli projektowej.
/// </summary>
public interface IPermissionService
{
    Task<bool> HasProjectPermissionAsync(Guid userId, Guid projectId, ResourceType resource, ActionType action,
        CancellationToken ct = default);

    /// <summary>
    /// Sprawdza uprawnienie bieżącego użytkownika.
    /// Rzuca UnauthenticatedException (401), gdy nikt nie jest zalogowany, i ForbiddenException (403) przy braku uprawnienia.
    /// </summary>
    Task EnsureProjectPermissionAsync(Guid projectId, ResourceType resource, ActionType action,
        CancellationToken ct = default);
}
