using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCowork.Domain.Models.Project;

namespace ProjectCowork.Persistence.Configuration;

internal class ProjectEntityConfiguration : IEntityTypeConfiguration<ProjectEntity>
{
    public void Configure(EntityTypeBuilder<ProjectEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Owner)
            .WithMany(x => x.OwnedProjects)
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(x => x.Collaborators)
            .WithMany(x => x.AssignedProjects)
            .UsingEntity(j => j.ToTable("ProjectCollaborators"));
    }
}