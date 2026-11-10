using System.Net;
using ProjectCowork.Domain.Exceptions.Abstractions;

namespace ProjectCowork.Domain.Exceptions.Github;

public class GithubApiException : ExceptionBase
{
    public GithubApiException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, statusCode)
    {
    }
}