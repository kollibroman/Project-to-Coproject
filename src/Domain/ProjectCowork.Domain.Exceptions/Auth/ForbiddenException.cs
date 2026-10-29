using System.Net;
using ProjectCowork.Domain.Exceptions.Abstractions;

namespace ProjectCowork.Domain.Exceptions.Auth;

public class ForbiddenException(string message, HttpStatusCode statusCode = HttpStatusCode.Forbidden)
    : ExceptionBase(message, statusCode);
