using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Models.Users;

namespace ProjectCowork.Persistence;

public class ProjectCoworkDbContext : IdentityDbContext<UserEntity, RoleEntity, Guid>
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ProjectCoworkDbContext).Assembly);
    }
}