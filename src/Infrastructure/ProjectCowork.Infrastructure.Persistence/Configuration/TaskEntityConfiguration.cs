using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCowork.Domain.Models.Management;

namespace ProjectCowork.Persistence.Configuration;

public class TaskEntityConfiguration : IEntityTypeConfiguration<TaskEntity>
{
    public void Configure(EntityTypeBuilder<TaskEntity> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.Project)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.ProjectId);
        
        builder.HasOne(x => x.Sprint)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.SprintId);
    }
}