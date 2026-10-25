using System;
using System.Linq;
using ProjectCowork.Domain.Models.Project;
using ProjectCowork.Domain.Models.Users;

namespace ProjectCowork.Infrastructure.Domain.Aggregates;

public class ProjectAggregate
{
    private ProjectEntity Project { get; }

    public ProjectAggregate(ProjectEntity project)
    {
        ArgumentNullException.ThrowIfNull(project);
        Project = project;
    }

    public static ProjectEntity Create(string name, string description, Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty.");
            
        var project = new ProjectEntity
        {
            Name = name,
            Description = description,
            OwnerId = ownerId,
            LastTaskFriendlyId = 0
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

    public void AddCollaborator(UserEntity user)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (Project.OwnerId == user.Id)
        {
            throw new InvalidOperationException("Owner is already part of the project.");
        }

        if (Project.Collaborators.Any(x => x.Id == user.Id))
        {
            throw new InvalidOperationException("User is already a collaborator of the project.");
        }

        Project.Collaborators.Add(user);
    }

    public void RemoveCollaborator(Guid userId)
    {
        var collaborator = Project.Collaborators.FirstOrDefault(x => x.Id == userId);

        if (collaborator is null)
        {
            return;
        }

        Project.Collaborators.Remove(collaborator);
    }

    public void DeleteProject()
    {
        // TODO: Business logic
    }
}