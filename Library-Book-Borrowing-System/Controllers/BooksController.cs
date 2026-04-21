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
        try
        {
            return Ok(bookService.CreateBook(book));
        }
        catch (Exception ex)
        {
            return StatusCode(400, ex.Message);
        }        
    }

    [HttpGet]
    public ActionResult<IEnumerable<GetBookResponse>> GetAllBooks()
    {
        return Ok(bookService.GetAllBooks());
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
        return Ok(bookService.UpdateBook(id, newBook));
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteBook(Guid id)
    {
        bookService.DeleteBook(id);
        return NoContent();
    }
}