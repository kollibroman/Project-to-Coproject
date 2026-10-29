using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCowork.Domain.Models.Project;

namespace ProjectCowork.Persistence.Configuration;

internal class ProjectRoleEntityConfiguration : IEntityTypeConfiguration<ProjectRoleEntity>
{

    public void Configure(EntityTypeBuilder<ProjectRoleEntity> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.Project)
            .WithMany(x => x.ProjectRoles)
            .HasForeignKey(x => x.ProjectId);

        builder.HasMany(x => x.FeatureActions)
            .WithMany(x => x.ProjectRoles);
    }
}