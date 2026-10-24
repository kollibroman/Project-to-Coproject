using System.Net;
using ProjectCowork.Domain.Exceptions.Abstractions;

namespace ProjectCowork.Domain.Exceptions.Auth;

public class InvalidTokenException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    : ExceptionBase(message, statusCode);