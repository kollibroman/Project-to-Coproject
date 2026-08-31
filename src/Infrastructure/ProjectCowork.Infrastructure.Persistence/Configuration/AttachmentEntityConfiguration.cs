using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCowork.Domain.Models.Attachments;

namespace ProjectCowork.Persistence.Configuration;

internal class AttachmentEntityConfiguration : IEntityTypeConfiguration<AttachmentEntity>
{
    public void Configure(EntityTypeBuilder<AttachmentEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.ProjectApplication)
            .WithMany(x => x.Attachments);
    }
}