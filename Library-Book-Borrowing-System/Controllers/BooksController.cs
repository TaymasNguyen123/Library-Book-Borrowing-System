using Microsoft.AspNetCore.Mvc;
using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Services;
using System.Runtime.InteropServices;

namespace Library_Book_Borrowing_System.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController(IBookService bookService) : ControllerBase
{
    [HttpPost]
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
    public ActionResult<GetBookResponse> UpdateBook([FromBody] dynamic Wrapper)
    {
        return Ok(bookService.UpdateBook(Wrapper.oldBook, Wrapper.book));
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteBook(Guid id)
    {
        bookService.DeleteBook(id);
        return NoContent();
    }
}