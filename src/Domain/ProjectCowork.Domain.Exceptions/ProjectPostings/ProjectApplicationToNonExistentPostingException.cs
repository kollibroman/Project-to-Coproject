using System.Net;
using ProjectCowork.Domain.Exceptions.Abstractions;

namespace ProjectCowork.Domain.Exceptions.ProjectPostings;

public class ProjectApplicationToNonExistentPostingException : ExceptionBase
{
    public ProjectApplicationToNonExistentPostingException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
    {
    }
}