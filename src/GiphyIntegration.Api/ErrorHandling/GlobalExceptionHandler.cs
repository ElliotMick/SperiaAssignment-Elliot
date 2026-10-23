using GiphyIntegration.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace GiphyIntegration.Api.ErrorHandling;

/// <summary>
/// Turns unhandled exceptions into RFC 7807 problem details without leaking internals.
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    // Non standard status (popularized by nginx) for requests the client gave up on.
    private const int ClientClosedRequest = 499;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            httpContext.Response.StatusCode = ClientClosedRequest;
            return true;
        }

        var (statusCode, title) = exception switch
        {
            GifSourceException => (StatusCodes.Status502BadGateway, "The GIF provider is currently unavailable."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
        };

        if (statusCode == StatusCodes.Status502BadGateway)
        {
            logger.LogWarning(exception, "GIF provider failure");
        }
        else
        {
            logger.LogError(exception, "Unhandled exception");
        }

        httpContext.Response.StatusCode = statusCode;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = { Status = statusCode, Title = title },
        });
    }
}
