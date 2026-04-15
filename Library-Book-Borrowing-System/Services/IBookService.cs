using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Models;

namespace Library_Book_Borrowing_System.Services;
public interface IBookService
{
    GetBookResponse CreateBook(CreateBookRequest book);
    IEnumerable<GetBookResponse> GetAllBooks();
    Task<GetBookDetailsResponse> GetBookById(Guid id);
    GetBookResponse UpdateBook(Book oldBook, UpdateBookRequest book);
    void DeleteBook(Guid id);
    
}