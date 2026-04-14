using Library_Book_Borrowing_System.Models;

namespace Library_Book_Borrowing_System.Repositories;

public interface IBookRepository
{
    Book CreateBook(Book book);
    IEnumerable<Book> GetAllBooks();
    Book GetBookById(Guid id);
    Book UpdateBook(
        string? updateTitle = null,
        string? updateAuthor = null,
        string? updateIsbn = null,
        int? updateTotalCopies = null,
        int? updateAvailableCopies = null
    );
    void DeleteBook(Guid id);
}