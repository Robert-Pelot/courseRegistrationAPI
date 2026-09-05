using CourseRegistration.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CourseRegistration.Api.Infrastructure;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    private static readonly Action<ILogger, Exception> LogUnhandledRequest = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(1, nameof(LogUnhandledRequest)),
        "Unhandled request failure");

    private static readonly Action<ILogger, int, string, Exception?> LogRejectedRequest = LoggerMessage.Define<int, string>(
        LogLevel.Information,
        new EventId(2, nameof(LogRejectedRequest)),
        "Request rejected with status {StatusCode}: {Message}");

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            RequestValidationException => (StatusCodes.Status400BadRequest, "Invalid request"),
            ResourceNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ResourceConflictException => (StatusCodes.Status409Conflict, "Resource conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected server error")
        };

        if (status >= 500)
        {
            LogUnhandledRequest(logger, exception);
        }
        else
        {
            LogRejectedRequest(logger, status, exception.Message, null);
        }

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status >= 500 ? "The request could not be completed." : exception.Message
            }
        });
    }
}
