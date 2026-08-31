using System.Net;
using ProjectCowork.Domain.Exceptions.Abstractions;

namespace ProjectCowork.Domain.Exceptions.ProjectPostings;

public class ProjectPostingWihWrongProjectException : ExceptionBase
{
    public ProjectPostingWihWrongProjectException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
    {
    }
}