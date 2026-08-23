using System.Net;

namespace ProjectCowork.Domain.Exceptions.Abstractions;

public abstract class ExceptionBase : Exception
{
    public HttpStatusCode StatusCode { get; }

    protected ExceptionBase(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        : base(message)
    {
        StatusCode = statusCode;
    }
}