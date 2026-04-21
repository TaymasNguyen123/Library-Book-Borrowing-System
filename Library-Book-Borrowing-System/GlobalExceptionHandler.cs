using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception, "Exception occurred: {Message}", exception.Message);
        
        if (exception is HttpRequestException httpEx)
        {
            int? statusCode = (int)(httpEx.StatusCode ?? System.Net.HttpStatusCode.InternalServerError);

            var problemDetails = new ProblemDetails
        
            {
                Status = statusCode,
                // Title = "Server error",
                Detail = httpEx.Message
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response
                .WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
        return false;        
    }
}