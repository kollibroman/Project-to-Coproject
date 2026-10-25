using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCowork.Domain.Models.Management;

namespace ProjectCowork.Persistence.Configuration;

public class SprintEntityConfiguration : IEntityTypeConfiguration<SprintEntity>
{
    public void Configure(EntityTypeBuilder<SprintEntity> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.Project)
            .WithMany(x => x.Sprints)
            .HasForeignKey(x => x.ProjectId);
        
        builder.HasMany(x => x.Tasks)
            .WithOne(x => x.Sprint)
            .HasForeignKey(x => x.SprintId);
    }
}