using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Library_Book_Borrowing_System.GlobalException;

public class GlobalExceptionHandler : IExceptionHandler
{
    public const string MISSING_BOOK_ID = "Book with that id does not exist";
    public const string MISSING_MEMBER_ID = "Member with that id does not exist";
    public const string INVALID_ISBN = "ISBN is invalid";
    public const string MORE_AVAILABLE_THAN_TOTAL = "Available copies cannot exceed total copies";
    public const string INVALID_EMAIL = "Email is invalid";
    public const string DUPLICATE_EMAIL = "Email already exists";
    public const string DUPLICATE_ISBN = "ISBN already exists";

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