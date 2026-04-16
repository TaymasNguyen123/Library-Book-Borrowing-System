using Microsoft.AspNetCore.Mvc;
using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Services;

namespace Library_Book_Borrowing_System.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController(IBookService bookService) : ControllerBase
{
    [HttpGet]
    public ActionResult<GetBookResponse> CreateBook(CreateBookRequest book)
    {
        return bookService.CreateBook(book);
    }

    [HttpGet]
    public ActionResult<IEnumerable<GetBookResponse>> GetAllBooks()
    {
        return Ok(bookService.GetAllBooks());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetBookResponse>> GetBookById(Guid id)
    {
        var response = await bookService.GetBookById(id);
        return Ok(response);
    }

    [HttpPost("{oldBook}/{book}")]
    public ActionResult<GetBookResponse> UpdateBook(Book oldBook, UpdateBookRequest book)
    {
        return Ok(bookService.UpdateBook(oldBook, book));
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteBook(Guid id)
    {
        bookService.DeleteBook(id);
        return NoContent();
    }
}