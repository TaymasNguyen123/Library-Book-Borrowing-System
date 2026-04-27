using Microsoft.AspNetCore.Mvc;
using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Services;

namespace Library_Book_Borrowing_System.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController(IBookService bookService) : ControllerBase
{
    [HttpPost]
    public ActionResult<GetBookResponse> CreateBook(CreateBookRequest book)
    {
        GetBookResponse? newBook = bookService.CreateBook(book);
        return CreatedAtAction(nameof(CreateBook), new { id = newBook.Id }, newBook);
    }

    [HttpGet]
    public ActionResult<PaginatedResponse<GetBookResponse>> GetAllBooks(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        return Ok(bookService.GetAllBooks(pageNumber, pageSize));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetBookDetailsResponse>> GetBookById(Guid id)
    {
        var response = await bookService.GetBookById(id);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public ActionResult<GetBookResponse> UpdateBook(Guid id, [FromBody] UpdateBookRequest newBook)
    {
        GetBookResponse? updatedBook = bookService.UpdateBook(id, newBook);
        return Ok(updatedBook);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteBook(Guid id)
    {
        bookService.DeleteBook(id);
        return NoContent();
    }
}
