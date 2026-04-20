using Library_Book_Borrowing_System.Models;

namespace Library_Book_Borrowing_System.Repositories;

public interface IBookRepository
{
    Book Add(Book book);
    IEnumerable<Book> GetAll();
    Book? GetById(Guid id);
    Book? GetByTitle(string title);
    Book? Update(Guid id, Book book);
    void Delete(Guid id);

    Task<bool> ExistsAsync(Guid bookId);
}