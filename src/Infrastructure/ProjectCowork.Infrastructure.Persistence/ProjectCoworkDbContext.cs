using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Models.Management;
using ProjectCowork.Domain.Models.Permissions;
using ProjectCowork.Domain.Models.Posting;
using ProjectCowork.Domain.Models.Project;
using ProjectCowork.Domain.Models.Repositories;
using ProjectCowork.Domain.Models.Users;

namespace ProjectCowork.Persistence;

public class ProjectCoworkDbContext : IdentityDbContext<UserEntity, RoleEntity, Guid>
{
    public ProjectCoworkDbContext()
    {
    }
    
    public ProjectCoworkDbContext(DbContextOptions<ProjectCoworkDbContext> options) : base(options)
    {
    }
    
    public virtual DbSet<ProjectEntity> Projects => Set<ProjectEntity>();
    public virtual DbSet<ActionEntity> Actions => Set<ActionEntity>();
    public virtual DbSet<ApplicationModuleEntity> Modules => Set<ApplicationModuleEntity>();
    public virtual DbSet<FeatureActionEntity> FeatureActions => Set<FeatureActionEntity>();
    public virtual DbSet<FeatureEntity> Features => Set<FeatureEntity>();
    public virtual DbSet<RolePermissionEntity> RolePermissions => Set<RolePermissionEntity>();
    public virtual DbSet<ProjectPostingEntity> ProjectPostings => Set<ProjectPostingEntity>();
    public virtual DbSet<ProjectApplicationEntity> ProjectApplications => Set<ProjectApplicationEntity>();
    public virtual DbSet<RepositoryEntity> Repositories => Set<RepositoryEntity>();
    public virtual DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
    public virtual DbSet<TaskEntity> Tasks => Set<TaskEntity>();
    public virtual DbSet<SprintEntity> Sprints => Set<SprintEntity>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ProjectCoworkDbContext).Assembly);
        
        base.OnModelCreating(builder);
    }
}