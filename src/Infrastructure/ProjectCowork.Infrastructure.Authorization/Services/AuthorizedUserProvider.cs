using ProjectCowork.Infrastructure.Authorization.Abstractions;

namespace ProjectCowork.Infrastructure.Authorization.Services;

internal class AuthorizedUserProvider : IAuthorizedUserProvider
{
    public Task<Guid> GetAuthorizedUserId()
    {
        throw new NotImplementedException();
    }
}