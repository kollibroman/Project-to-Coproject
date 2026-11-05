using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCowork.Domain.Models.Project;

namespace ProjectCowork.Persistence.Configuration;

internal class ProjectInvitationEntityConfiguration : IEntityTypeConfiguration<ProjectInvitationEntity>
{

    public void Configure(EntityTypeBuilder<ProjectInvitationEntity> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.Project)
            .WithMany(x => x.ProjectInvitations)
            .HasForeignKey(x => x.ProjectId);
        
        builder.HasOne(x => x.User)
            .WithMany(x => x.ProjectInvitations)
            .HasForeignKey(x => x.UserId);
    }
}