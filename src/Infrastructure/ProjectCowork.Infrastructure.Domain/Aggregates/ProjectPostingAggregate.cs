using ProjectCowork.Domain.Models.Posting;

namespace ProjectCowork.Infrastructure.Domain.Aggregates;

public class ProjectPostingAggregate
{
    private ProjectPostingEntity ProjectPosting { get; }
    
    public ProjectPostingAggregate(ProjectPostingEntity projectPosting)
    {
        ArgumentNullException.ThrowIfNull(projectPosting);
        ProjectPosting = projectPosting;
    }

    public static ProjectPostingEntity Create(Guid projectId, string jobDescription, string projectDescription)
    {
        return new ProjectPostingEntity
        {
            JobDescription = jobDescription,
            ProjectDescription = projectDescription,
            ProjectId = projectId
        };
    }
    
    public void SetJobDescription(string jobDescription)
    {
        ProjectPosting.JobDescription = jobDescription;
    }
    
    public void SetProjectDescription(string projectDescription)
    {
        ProjectPosting.ProjectDescription = projectDescription;
    }

    public void SetActivityStatus(bool activityStatus)
    {
        ProjectPosting.IsActive =  activityStatus;
    }
}
