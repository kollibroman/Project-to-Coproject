using System.Net;
using Microsoft.EntityFrameworkCore;
using ProjectCowork.Domain.Exceptions.ProjectPostings;
using ProjectCowork.Domain.Models.Posting;
using ProjectCowork.Persistence;

namespace ProjectCowork.Infrastructure.Domain.Aggregates;

public class ProjectPostingAggregate
{
    private ProjectPostingEntity ProjectPosting { get; }
    
    public ProjectPostingAggregate(ProjectPostingEntity projectPosting)
    {
        ProjectPosting = projectPosting;
    }

    public static async Task<ProjectPostingEntity> Create(ProjectCoworkDbContext context, Guid projectId, string jobDescription, string projectDescription)
    {
        var projectExists = await context.Projects
            .AsNoTracking()
            .AnyAsync(x => x.Id == projectId);

        if (!projectExists)
        {
            throw new ProjectPostingWihWrongProjectException("Project doesn't exist", HttpStatusCode.BadRequest);
        }

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