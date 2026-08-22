namespace ProjectCowork.Domain.Exceptions.Common;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message)
    {
    }
}