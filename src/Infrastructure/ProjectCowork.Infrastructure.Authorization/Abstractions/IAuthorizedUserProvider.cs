namespace ProjectCowork.Infrastructure.Authorization.Abstractions;

public interface IAuthorizedUserProvider
{
    public Task<Guid> GetAuthorizedUserId();
}