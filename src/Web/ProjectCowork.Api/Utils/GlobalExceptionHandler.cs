using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProjectCowork.Domain.Exceptions.Abstractions;

namespace ProjectCowork.Api.Utils;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
        
        var problem = new ProblemDetails
        {
            Title = "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Please contact support if the problem persists."
        };

        if (exception is ExceptionBase exceptionBase)
        {
            problem = new ProblemDetails
            {
                Title = exceptionBase.Message,
                Status = (int)exceptionBase.StatusCode,
                Detail = exceptionBase.StackTrace
            };
        }

        context.Response.StatusCode = problem.Status.Value;

        await context.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}