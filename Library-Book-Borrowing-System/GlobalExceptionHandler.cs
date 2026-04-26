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
    public const string MISSING_BORROW_RECORD = "Member has not borrowed this book";
    public const string DUPLICATE_RECORD = "Book is already being borrowed by member";
    public const string ZERO_COPIES_AVAILABLE = "Zero copies Available";
    public const string NO_BOOKS_FOUND = "No books found matching the search criteria";
    public const string BOOK_IS_BORROWED = "Cannot delete book becuase it is currently being borrowed";
    public const string NOT_FOUND = "Please provide at least one search criteria (Title, Author, or ISBN).";
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