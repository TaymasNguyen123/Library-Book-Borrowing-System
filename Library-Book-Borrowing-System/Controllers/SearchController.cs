using Microsoft.AspNetCore.Mvc;
using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Services;
using System.Diagnostics.CodeAnalysis;


namespace Library_Book_Borrowing_System.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly IBookService _bookService;

    public SearchController(IBookService bookService)
    {
        _bookService = bookService;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetBookResponse>>> SearchBooks(
        [FromQuery] string? title, 
        [FromQuery] string? author,
        [FromQuery] string? isbn)
    {
        var results = await _bookService.SearchBooksAsync(title, author, isbn);
        return Ok(results);
    }

}