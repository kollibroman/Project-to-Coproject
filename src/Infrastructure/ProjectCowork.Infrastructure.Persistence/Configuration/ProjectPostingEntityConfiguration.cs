using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCowork.Domain.Models.Posting;

namespace ProjectCowork.Persistence.Configuration;

internal class ProjectPostingEntityConfiguration : IEntityTypeConfiguration<ProjectPostingEntity>
{
    public void Configure(EntityTypeBuilder<ProjectPostingEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasMany(x => x.Applications)
            .WithOne(x => x.ProjectPosting);

        builder.HasOne(x => x.Project)
            .WithMany(x => x.ProjectPostings);
    }
}