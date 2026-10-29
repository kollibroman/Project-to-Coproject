using System.Net;
using ProjectCowork.Domain.Exceptions.Abstractions;

namespace ProjectCowork.Domain.Exceptions.Projects;

public class ProjectAccessException(string message, HttpStatusCode statusCode = HttpStatusCode.Conflict)
    : ExceptionBase(message, statusCode);
