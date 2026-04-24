using Microsoft.AspNetCore.Mvc;
using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;
using Library_Book_Borrowing_System.Services;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authorization;

namespace Library_Book_Borrowing_System.Controllers;

[ApiController]
[Route("api/books")]
[Authorize]
public class BooksController(IBookService bookService) : ControllerBase
{
    [HttpPost]
    public ActionResult<GetBookResponse> CreateBook(CreateBookRequest book)
    {
        GetBookResponse? newBook = bookService.CreateBook(book);
        return CreatedAtAction(nameof(CreateBook), new { id = newBook.Id }, newBook);
    }

    [AllowAnonymous]
    [HttpGet]
    public ActionResult<IEnumerable<GetBookResponse>> GetAllBooks()
    {
        return Ok(bookService.GetAllBooks());
    }

    [AllowAnonymous]
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