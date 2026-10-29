using System.Net;
using ProjectCowork.Domain.Exceptions.Abstractions;

namespace ProjectCowork.Domain.Exceptions.Common;

public class EntityAlreadyExistsException(string message, HttpStatusCode statusCode = HttpStatusCode.Conflict)
    : ExceptionBase(message, statusCode);
