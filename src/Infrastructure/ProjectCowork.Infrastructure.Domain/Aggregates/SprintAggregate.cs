using ProjectCowork.Domain.Models.Management;
using ProjectCowork.Infrastructure.Domain.Abstractions;
using ProjectCowork.Infrastructure.DomainEvents.Management;

namespace ProjectCowork.Infrastructure.Domain.Aggregates;

public class SprintAggregate : AggregateRoot
{
    private SprintEntity Sprint { get; }

    public SprintAggregate(SprintEntity sprint)
    {
        ArgumentNullException.ThrowIfNull(sprint);
        Sprint = sprint;
    }

    public static SprintEntity Create(Guid projectId, string name, DateOnly startDate, DateOnly endDate)
    {
        ValidateName(name);
        ValidateDates(startDate, endDate);

        return new SprintEntity
        {
            Name = name,
            StartDate = startDate,
            EndDate = endDate,
            ProjectId = projectId
        };
    }

    public void SetName(string name)
    {
        if (Sprint.Name == name)
        {
            return;
        }

        ValidateName(name);
        Sprint.Name = name;
    }

    public void SetDates(DateOnly startDate, DateOnly endDate)
    {
        ValidateDates(startDate, endDate);

        Sprint.StartDate = startDate;
        Sprint.EndDate = endDate;
    }

    public void DeleteSprint()
    {
        RaiseDomainEvent(new SprintDeletedEvent
        {
            SprintId = Sprint.Id,
            ProjectId = Sprint.ProjectId
        });
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Sprint name cannot be empty.");
        }
    }

    private static void ValidateDates(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("Sprint end date cannot be earlier than start date.");
        }
    }
}
