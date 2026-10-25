using System.Net;
using ProjectCowork.Domain.Exceptions.ProjectPostings;
using ProjectCowork.Domain.Models.Attachments;
using ProjectCowork.Domain.Models.Posting;

namespace ProjectCowork.Infrastructure.Domain.Aggregates;

public class ProjectApplicationAggregate
{
    private ProjectApplicationEntity ProjectApplication { get; }
    private ICollection<AttachmentEntity> Attachments { get; }
    
    public ProjectApplicationAggregate(ProjectApplicationEntity projectApplication, ICollection<AttachmentEntity> attachments)
    {
        ArgumentNullException.ThrowIfNull(projectApplication);
        ProjectApplication = projectApplication;
        Attachments = attachments;
    }

    public static ProjectApplicationEntity Create(ProjectPostingEntity posting, string description, Guid userId)
    {
        ArgumentNullException.ThrowIfNull(posting);

        if (!posting.IsActive)
        {
            throw new ProjectApplicationToNonExistentPostingException($"Posting {posting.Id} is inactive",
                HttpStatusCode.BadRequest);
        }

        return new ProjectApplicationEntity
        {
            Description = description,
            UserId = userId,
            ProjectPostingId = posting.Id
        };
    }

    public void SetApplicationAttachments()
    {
        ProjectApplication.Attachments = Attachments;
    }
}
