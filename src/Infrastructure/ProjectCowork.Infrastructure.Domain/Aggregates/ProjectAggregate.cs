using System;
using ProjectCowork.Domain.Exceptions.Common;
using ProjectCowork.Domain.Models.Project;

namespace ProjectCowork.Infrastructure.Domain.Aggregates;

public class ProjectAggregate
{
    private ProjectEntity Project { get; }

    public ProjectAggregate(ProjectEntity? project)
    {
        Project = project ?? throw new EntityNotFoundException("Project not found");
    }

    public static ProjectEntity Create(string name, string description, Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty.");
            
        var project = new ProjectEntity
        {
            Name = name,
            Description = description,
            OwnerId = ownerId
        };

        return project;
    }

    public void SetProjectName(string name)
    {
        if (Project.Name == name)
        {
            return;
        }
        
        Project.Name = name;
    }
    
    public void SetDescription(string description)
    {
        if (Project.Description == description)
        {
            return;
        }

        Project.Description = description;
    }

    public void DeleteProject()
    {
        // TODO: Business logic
    }
}