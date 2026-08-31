using System.Net;
using DispatchR;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Exceptions.ProjectPostings;
using ProjectCowork.Domain.Models.Attachments;
using ProjectCowork.Domain.Models.Posting;
using ProjectCowork.Infrastructure.DomainEvents.Attachments;
using ProjectCowork.Persistence;

namespace ProjectCowork.Infrastructure.Domain.Aggregates;

public class ProjectApplicationAggregate
{
    private ProjectApplicationEntity ProjectApplication { get; }
    private ICollection<AttachmentEntity> Attachments { get; }
    
    public ProjectApplicationAggregate(ProjectApplicationEntity projectApplication, ICollection<AttachmentEntity> attachments)
    {
        ProjectApplication = projectApplication;
        Attachments = attachments;
    }

    public static async Task<ProjectApplicationEntity> Create(ProjectCoworkDbContext context, string description,
        Guid userId, Guid projectPostingId)
    {
        var postingExists = await context.ProjectPostings
            .AsNoTracking()
            .AnyAsync(x => x.IsActive && x.Id == projectPostingId);

        if (!postingExists)
        {
            throw new ProjectApplicationToNonExistentPostingException($"Posting is inactive or doesn't exist {projectPostingId}", HttpStatusCode.BadRequest);
        }

        return new ProjectApplicationEntity
        {
            Description = description,
            UserId = userId,
            ProjectPostingId = projectPostingId
        };
    }

    public void SetApplicationAttachmentsAsync()
    {
        ProjectApplication.Attachments = Attachments;
    }
}