using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCowork.Domain.Models.Repositories;

namespace ProjectCowork.Persistence.Configuration;

public class RepositoryEntityConfiguration : IEntityTypeConfiguration<RepositoryEntity>
{

    public void Configure(EntityTypeBuilder<RepositoryEntity> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.Project)
            .WithMany(x => x.Repositories)
            .HasForeignKey(x => x.ProjectId);
    }
}