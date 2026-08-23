using System.Net;
using ProjectCowork.Domain.Exceptions.Abstractions;

namespace ProjectCowork.Domain.Exceptions.Common;

public class EntityNotFoundException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    : ExceptionBase(message, statusCode);