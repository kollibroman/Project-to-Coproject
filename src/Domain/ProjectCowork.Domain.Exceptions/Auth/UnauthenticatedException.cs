using System.Net;
using ProjectCowork.Domain.Exceptions.Abstractions;

namespace ProjectCowork.Domain.Exceptions.Auth;

public class UnauthenticatedException(string message, HttpStatusCode statusCode = HttpStatusCode.Unauthorized)
    : ExceptionBase(message, statusCode);
