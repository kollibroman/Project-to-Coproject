using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCowork.Domain.Models.Project;

namespace ProjectCowork.Persistence.Configuration;

internal class ProjectMemberEntityConfiguration : IEntityTypeConfiguration<ProjectMemberEntity>
{

    public void Configure(EntityTypeBuilder<ProjectMemberEntity> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.Project)
            .WithMany(x => x.ProjectMembers)
            .HasForeignKey(x => x.ProjectId);
        
        builder.HasOne(x => x.User)
            .WithMany(x => x.AssignedProjects)
            .HasForeignKey(x => x.UserId);
    }
}