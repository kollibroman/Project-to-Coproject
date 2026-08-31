using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCowork.Domain.Models.Posting;

namespace ProjectCowork.Persistence.Configuration;

internal class ProjectApplicationEntityConfiguration : IEntityTypeConfiguration<ProjectApplicationEntity>
{
    public void Configure(EntityTypeBuilder<ProjectApplicationEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
            .WithMany(x => x.ProjectApplications);
        
        builder.HasMany(x => x.Attachments)
            .WithOne(x => x.ProjectApplication);

        builder.HasOne(x => x.ProjectPosting)
            .WithMany(x => x.Applications);
    }
}