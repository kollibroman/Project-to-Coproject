namespace ProjectCowork.Domain.Models.Users;

public class RefreshTokenEntity : BaseTrackedEntity
{
    public Guid Id { get; }
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; init; }
    
    public Guid UserId { get; set; }
    public UserEntity User { get; set; }
}